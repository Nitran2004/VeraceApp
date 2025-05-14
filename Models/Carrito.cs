namespace ProyectoIdentity.Models
{
   public class Carrito
    {
        public int Id { get; set; }
        public string? UsuarioId { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }

    }

}
