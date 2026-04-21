using System.ComponentModel;

namespace GarageManagement.ServiceOrders;

public enum ServiceOrderStatus
{
    [Description("Recebida")]
    Received = 1,

    [Description("Em Diagnóstico")]
    InDiagnosis = 2,

    [Description("Em Execução")]
    InExecution = 3,

    [Description("Aguardando Aprovação")]
    WaitingApproval = 4,

    [Description("Aguardando Execução")]
    WaitingExecution = 5,

    [Description("Finalizada")]
    Finished = 6,

    [Description("Entregue")]
    Delivered = 7,

    [Description("Fechada")]
    Closed = 8,

    [Description("Cancelada")]
    Canceled = 9
}
