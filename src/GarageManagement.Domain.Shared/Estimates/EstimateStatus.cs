using System.ComponentModel;

namespace GarageManagement.Estimates;

public enum EstimateStatus
{
    [Description("Rascunho")]
    Draft = 1,
    
    [Description("Aguardando Aprovação")]
    PendingApproval = 2,

    [Description("Aprovado")]
    Approved = 3,
    [Description("Recusado")]
    Rejected = 4
}
