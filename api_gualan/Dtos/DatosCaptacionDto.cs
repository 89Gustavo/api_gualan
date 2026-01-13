public class DatosCaptacionDto
{
    public string? codigo_empresa { get; set; }
    public string? nombre_empresa { get; set; }
    public string? cliente { get; set; }
    public string? numero_cuenta { get; set; }
    public string? arrangement { get; set; }
    public string? categoria { get; set; }
    public string? producto { get; set; }
    public string? grupo_producto { get; set; }
    public string? moneda { get; set; }

    public decimal? saldo { get; set; }              // Nullable decimal
    public string? estado { get; set; }
    public DateTime? fecha_apertura { get; set; }    // Nullable DateTime
    public int? plazo { get; set; }                  // Nullable int
    public DateTime? vencimiento { get; set; }

    public decimal? tasa { get; set; }
    public decimal? interes_acumulado { get; set; }
    public decimal? monto_reserva { get; set; }

    public string? ejecutivo { get; set; }
    public string? frecuencia_pago { get; set; }
    public DateTime? fecha_ultimo_credito { get; set; }
    public DateTime? fecha_ultimo_debito { get; set; }
    public string? agencia { get; set; }
    public string? usuario_act { get; set; }
    public decimal? monto_disponible { get; set; }
    public string? referencia { get; set; }
    public string? tipo_cliente { get; set; }
    public DateTime? fecha_renovacion { get; set; }
    public decimal? monto_apertura { get; set; }
    public string? cuenta_dep_int { get; set; }
    public string? nombre_cuenta_depint { get; set; }
    public decimal? interes_x_pagar { get; set; }
    public string? tarjeta_asociada { get; set; }
    public string? categoria_producto { get; set; }
    public string? clasificacion { get; set; }
    public string? grupo { get; set; }
    public string? usuario_creacion_cta { get; set; }
    public decimal? encaje { get; set; }
    public decimal? monto_pignorado { get; set; }
}
