//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pulperia.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ventas
    {
        public int Id { get; set; }
        public int IdComprador { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public System.DateTime FechaCompra { get; set; }
    
        public virtual Compradores Compradores { get; set; }
        public virtual Productos Productos { get; set; }
    }
}
