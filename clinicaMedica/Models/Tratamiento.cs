using System;
using System.Collections.Generic;

namespace clinicaMedica.Models;

public partial class Tratamiento
{
    public int Id { get; set; }

    public int? CitaId { get; set; }

    public string? Notas { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Cita? Cita { get; set; }
}
