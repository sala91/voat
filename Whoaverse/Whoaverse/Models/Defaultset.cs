//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Whoaverse.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Defaultset
    {
        public Defaultset()
        {
            this.Defaultsetsetups = new HashSet<Defaultsetsetup>();
        }
    
        public int Set_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Created_by { get; set; }
        public System.DateTime Created_on { get; set; }
        public bool Public { get; set; }
        public int Order { get; set; }
    
        public virtual ICollection<Defaultsetsetup> Defaultsetsetups { get; set; }
    }
}
