﻿using OpenGraph_Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;

namespace Voat.Business.Utilities
{
    public class HttpResource : IDisposable
    {
        private Uri _uri = null;
        private Uri _redirectedUri = null;
        private HttpResponseMessage _response;
        private MemoryStream _stream;
        private TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private string _title = null;
        private string _contentString = null;
        private Uri _image = null;

        public HttpResponseMessage Response { get => _response; }
        public Stream Stream { get => _stream;  }
        public TimeSpan Timeout { get => _timeout; set => _timeout = value; }
        public Uri Uri { get => _uri; }
        public Uri RedirectedUri { get => _redirectedUri; }
        public bool Redirected { get => _uri != _redirectedUri; }

        public HttpResource(Uri uri)
        {
            _uri = uri;
        }
        public HttpResource(string uri) : this(new Uri(uri))
        {
        }
        public async Task Execute(bool allowAutoRedirect = false, HttpCompletionOption options = HttpCompletionOption.ResponseContentRead)
        {
            var handler = new HttpClientHandler() { AllowAutoRedirect = allowAutoRedirect };

            using (var httpClient = new HttpClient(handler))
            {
                httpClient.Timeout = _timeout;
                httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue($"Voat-OpenGraph-Parser", "2"));

                _response = await httpClient.GetAsync(Uri, options);
                _redirectedUri = _response.RequestMessage.RequestUri;

                if (options == HttpCompletionOption.ResponseContentRead)
                {
                    //Copy Response
                    _stream = new MemoryStream();
                    await _response.Content.CopyToAsync(_stream);
                    _stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
        public Uri Image
        {
            get
            {
                if (_image == null)
                {
                    //Check if this url is an image extension
                    var imageExtensions = new string[] { ".jpg", ".png", ".gif", ".jpeg" };
                    var file = Path.GetExtension(Uri.ToString());
                    if (imageExtensions.Any(x => x.IsEqual(file)))
                    {
                        _image = _uri;
                        return _image;
                    }

                    EnsureReady();
                    //Check OpenGraph
                    var graph = OpenGraph.ParseHtml(ContentString);
                    if (graph.Image != null)
                    {
                        _image = graph.Image;
                        return _image;
                    }
                }
                return _image;
            }
        }
        public string Title
        {
            get
            {
                EnsureReady();

                if (_title == null)
                {
                    //Try Open Graph
                    var graph = OpenGraph.ParseHtml(ContentString);
                    if (!String.IsNullOrEmpty(graph.Title))
                    {
                        _title = WebUtility.HtmlDecode(graph.Title);
                    }
                    //Try Getting from Title
                    if (String.IsNullOrEmpty(_title))
                    {
                        var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(ContentString);
                        var titleNode = htmlDocument.DocumentNode.Descendants("title").SingleOrDefault();
                        if (titleNode != null)
                        {
                            _title = WebUtility.HtmlDecode(titleNode.InnerText);
                        }
                    }
                }

                return _title;
            }
        }
        private void EnsureReady()
        {
            if (this.Response == null || !this.Response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Request has not been processed");
            }
        }
        private string ContentString
        {
            get
            {
                EnsureReady();
                if (_contentString == null)
                {
                    var reader = new StreamReader(this.Stream);
                    _contentString = reader.ReadToEnd();
                }
                return _contentString;
            }
        }
        public void Dispose()
        {
            _response?.Dispose();
            _stream?.Dispose();
        }
    }
}
