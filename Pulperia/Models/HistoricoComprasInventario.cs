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
    
    public partial class HistoricoComprasInventario
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadInventario { get; set; }
        public int CantidadComprada { get; set; }
        public System.DateTime FechaCompra { get; set; }
        public System.DateTime FechaVencimiento { get; set; }
        public string Lote { get; set; }
        public decimal PrecioInvividual { get; set; }
        public int Mes { get; set; }
        public int Anno { get; set; }
    }
}
