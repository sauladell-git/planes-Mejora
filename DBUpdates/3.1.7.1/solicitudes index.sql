CREATE NONCLUSTERED INDEX [Index_Rep1]
ON [dbo].[Solicitudes] ([ImprovementPlanId])
INCLUDE ([Id],[StatusId],[RequestedAmount],[RequestedPriceUnit],[ApprovedAmount],[ApprovedPriceUnit])
GO
CREATE NONCLUSTERED INDEX [Index_Rep2]
ON [dbo].[Dictums_Solicitudes] ([SolicitudeId])
INCLUDE ([DictumId])
GO
CREATE NONCLUSTERED INDEX [Index_SolicitudesLines]
ON [dbo].[Solicitudes] ([LineId])
INCLUDE ([Id])
GO
CREATE NONCLUSTERED INDEX [Index_SolicitudesPlan]
ON [dbo].[Solicitudes] ([ImprovementPlanId])
INCLUDE ([Id])
GO
CREATE NONCLUSTERED INDEX [Index_ResolutionDictums]
ON [dbo].[ResolutionDictums] ([DictumId])
INCLUDE ([ResolutionId])
GO
CREATE NONCLUSTERED INDEX [Index_Documents]
ON [dbo].[Documents] ([ImprovementPlanId])
INCLUDE ([Id])
GO