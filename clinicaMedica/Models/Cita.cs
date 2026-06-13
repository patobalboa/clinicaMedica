using System;
using System.Collections.Generic;

namespace clinicaMedica.Models;

public partial class Cita
{
    public int Id { get; set; }

    public int? PacienteId { get; set; }

    public int? MedicoId { get; set; }

    public int? EspecialidadId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Especialidad? Especialidad { get; set; }

    public virtual Medico? Medico { get; set; }

    public virtual Paciente? Paciente { get; set; }

    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}
