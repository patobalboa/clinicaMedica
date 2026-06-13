using System;
using System.Collections.Generic;

namespace clinicaMedica.Models;

public partial class Especialidad
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();
}
