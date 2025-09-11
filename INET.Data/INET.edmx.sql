
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/19/2021 18:03:15
-- Generated from EDMX file: C:\GIT\INET_Full_20190623\INET.Data\INET.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [INET_PROD_V3];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Audits_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Audits] DROP CONSTRAINT [FK_Audits_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_Budgets_Fields]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Budgets] DROP CONSTRAINT [FK_Budgets_Fields];
GO
IF OBJECT_ID(N'[dbo].[FK_Budgets_Lines]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Budgets] DROP CONSTRAINT [FK_Budgets_Lines];
GO
IF OBJECT_ID(N'[dbo].[FK_Budgets_Provinces]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Budgets] DROP CONSTRAINT [FK_Budgets_Provinces];
GO
IF OBJECT_ID(N'[dbo].[FK_Budgets_SchoolYears]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Budgets] DROP CONSTRAINT [FK_Budgets_SchoolYears];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_ImprovementPlans_Comments]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_ImprovementPlans] DROP CONSTRAINT [FK_Comments_ImprovementPlans_Comments];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_ImprovementPlans_ImprovementPlans]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_ImprovementPlans] DROP CONSTRAINT [FK_Comments_ImprovementPlans_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_Incidences_Comments]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_Incidences] DROP CONSTRAINT [FK_Comments_Incidences_Comments];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_Incidences_Incidences]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_Incidences] DROP CONSTRAINT [FK_Comments_Incidences_Incidences];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_Solicitudes_Comments]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_Solicitudes] DROP CONSTRAINT [FK_Comments_Solicitudes_Comments];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_Solicitudes_Solicitudes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments_Solicitudes] DROP CONSTRAINT [FK_Comments_Solicitudes_Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_Comments_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_Dictums_Solicitudes_Dictums]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Dictums_Solicitudes] DROP CONSTRAINT [FK_Dictums_Solicitudes_Dictums];
GO
IF OBJECT_ID(N'[dbo].[FK_Dictums_Solicitudes_Solicitudes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Dictums_Solicitudes] DROP CONSTRAINT [FK_Dictums_Solicitudes_Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[FK_Documents_ImprovementPlans]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Documents] DROP CONSTRAINT [FK_Documents_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[FK_Documents_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Documents] DROP CONSTRAINT [FK_Documents_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_DocumentVariables_Documents]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocumentVariables] DROP CONSTRAINT [FK_DocumentVariables_Documents];
GO
IF OBJECT_ID(N'[dbo].[FK_DocumentVariables_TemplateTypeVariables]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DocumentVariables] DROP CONSTRAINT [FK_DocumentVariables_TemplateTypeVariables];
GO
IF OBJECT_ID(N'[dbo].[FK_FieldStatus]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Fields] DROP CONSTRAINT [FK_FieldStatus];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_Fields]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_Fields];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_ImprovementPlans]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_ImprovementPlansTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_ImprovementPlansTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_SchoolYears]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_SchoolYears];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_Status]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_Status];
GO
IF OBJECT_ID(N'[dbo].[FK_ImprovementPlans_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ImprovementPlans] DROP CONSTRAINT [FK_ImprovementPlans_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_Incidences_ImprovementPlans]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Incidences] DROP CONSTRAINT [FK_Incidences_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[FK_Incidences_IncidenceTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Incidences] DROP CONSTRAINT [FK_Incidences_IncidenceTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_Incidences_Solicitudes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Incidences] DROP CONSTRAINT [FK_Incidences_Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[FK_Incidences_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Incidences] DROP CONSTRAINT [FK_Incidences_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_Lines_Fields]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lines] DROP CONSTRAINT [FK_Lines_Fields];
GO
IF OBJECT_ID(N'[dbo].[FK_ResolutionDictums_Dictums]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ResolutionDictums] DROP CONSTRAINT [FK_ResolutionDictums_Dictums];
GO
IF OBJECT_ID(N'[dbo].[FK_ResolutionDictums_Resolutions]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ResolutionDictums] DROP CONSTRAINT [FK_ResolutionDictums_Resolutions];
GO
IF OBJECT_ID(N'[dbo].[FK_Resolutions_Documents]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Resolutions] DROP CONSTRAINT [FK_Resolutions_Documents];
GO
IF OBJECT_ID(N'[dbo].[fk_RoleId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [fk_RoleId];
GO
IF OBJECT_ID(N'[dbo].[FK_RolePermissionStatuses_webpages_Roles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_RolePermissionStatuses] DROP CONSTRAINT [FK_RolePermissionStatuses_webpages_Roles];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_ExpenditureTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_ExpenditureTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_ImprovementPlans]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_Lines]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_Lines];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_MeasurementUnits]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_MeasurementUnits];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_SchoolYears]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_SchoolYears];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_Solicitudes1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_Solicitudes1];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_SolicitudeTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_SolicitudeTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_Solicitudes_Status]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Solicitudes] DROP CONSTRAINT [FK_Solicitudes_Status];
GO
IF OBJECT_ID(N'[dbo].[FK_Status_StatusCriterions]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Status] DROP CONSTRAINT [FK_Status_StatusCriterions];
GO
IF OBJECT_ID(N'[dbo].[FK_TemplateField_Field]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TemplateField] DROP CONSTRAINT [FK_TemplateField_Field];
GO
IF OBJECT_ID(N'[dbo].[FK_TemplateField_Template]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TemplateField] DROP CONSTRAINT [FK_TemplateField_Template];
GO
IF OBJECT_ID(N'[dbo].[FK_Templates_TemplateTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Templates] DROP CONSTRAINT [FK_Templates_TemplateTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_TemplateTypeBlocks_TemplateTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TemplateTypeBlocks] DROP CONSTRAINT [FK_TemplateTypeBlocks_TemplateTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_TemplateTypeFields_TemplateTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TemplateTypeFields] DROP CONSTRAINT [FK_TemplateTypeFields_TemplateTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_TemplateTypeVariables_Templates]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TemplateVariables] DROP CONSTRAINT [FK_TemplateTypeVariables_Templates];
GO
IF OBJECT_ID(N'[dbo].[fk_UserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [fk_UserId];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_Fields_Fields]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_Fields] DROP CONSTRAINT [FK_UserProfile_Fields_Fields];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_Fields_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_Fields] DROP CONSTRAINT [FK_UserProfile_Fields_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_InstitutionLevels_InstitutionLevels]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_InstitutionLevels] DROP CONSTRAINT [FK_UserProfile_InstitutionLevels_InstitutionLevels];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_InstitutionLevels_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_InstitutionLevels] DROP CONSTRAINT [FK_UserProfile_InstitutionLevels_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_Provinces_Provinces]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_Provinces] DROP CONSTRAINT [FK_UserProfile_Provinces_Provinces];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfile_Provinces_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile_Provinces] DROP CONSTRAINT [FK_UserProfile_Provinces_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[fk_webpages_Permissions_StatusCriterions_PermissionId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_Permissions_StatusCriterions] DROP CONSTRAINT [fk_webpages_Permissions_StatusCriterions_PermissionId];
GO
IF OBJECT_ID(N'[dbo].[fk_webpages_Permissions_StatusCriterions_StatusCriterionsId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_Permissions_StatusCriterions] DROP CONSTRAINT [fk_webpages_Permissions_StatusCriterions_StatusCriterionsId];
GO
IF OBJECT_ID(N'[dbo].[FK_webpages_RolePermissionStatuses_Status]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_RolePermissionStatuses] DROP CONSTRAINT [FK_webpages_RolePermissionStatuses_Status];
GO
IF OBJECT_ID(N'[dbo].[FK_webpages_RolePermissionStatuses_webpages_Permissions]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_RolePermissionStatuses] DROP CONSTRAINT [FK_webpages_RolePermissionStatuses_webpages_Permissions];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Audits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Audits];
GO
IF OBJECT_ID(N'[dbo].[Budgets]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Budgets];
GO
IF OBJECT_ID(N'[dbo].[Comments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comments];
GO
IF OBJECT_ID(N'[dbo].[Comments_ImprovementPlans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comments_ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[Comments_Incidences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comments_Incidences];
GO
IF OBJECT_ID(N'[dbo].[Comments_Solicitudes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comments_Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[Dictums]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Dictums];
GO
IF OBJECT_ID(N'[dbo].[Dictums_Solicitudes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Dictums_Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[DictumTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DictumTypes];
GO
IF OBJECT_ID(N'[dbo].[Documents]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Documents];
GO
IF OBJECT_ID(N'[dbo].[DocumentVariables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DocumentVariables];
GO
IF OBJECT_ID(N'[dbo].[ELMAH_Error]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ELMAH_Error];
GO
IF OBJECT_ID(N'[dbo].[ExpenditureTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExpenditureTypes];
GO
IF OBJECT_ID(N'[dbo].[Fields]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Fields];
GO
IF OBJECT_ID(N'[dbo].[FileNumbers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FileNumbers];
GO
IF OBJECT_ID(N'[dbo].[ImprovementPlans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ImprovementPlans];
GO
IF OBJECT_ID(N'[dbo].[ImprovementPlansTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ImprovementPlansTypes];
GO
IF OBJECT_ID(N'[dbo].[Incidences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Incidences];
GO
IF OBJECT_ID(N'[dbo].[IncidenceTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IncidenceTypes];
GO
IF OBJECT_ID(N'[dbo].[InstitutionLevels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InstitutionLevels];
GO
IF OBJECT_ID(N'[dbo].[Institutions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Institutions];
GO
IF OBJECT_ID(N'[dbo].[Lines]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Lines];
GO
IF OBJECT_ID(N'[dbo].[MeasurementUnits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MeasurementUnits];
GO
IF OBJECT_ID(N'[dbo].[Provinces]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Provinces];
GO
IF OBJECT_ID(N'[dbo].[ResolutionDictums]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ResolutionDictums];
GO
IF OBJECT_ID(N'[dbo].[Resolutions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Resolutions];
GO
IF OBJECT_ID(N'[dbo].[SchoolYears]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SchoolYears];
GO
IF OBJECT_ID(N'[dbo].[Solicitudes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Solicitudes];
GO
IF OBJECT_ID(N'[dbo].[SolicitudeTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SolicitudeTypes];
GO
IF OBJECT_ID(N'[dbo].[Status]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Status];
GO
IF OBJECT_ID(N'[dbo].[StatusCriterions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StatusCriterions];
GO
IF OBJECT_ID(N'[dbo].[TemplateField]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TemplateField];
GO
IF OBJECT_ID(N'[dbo].[Templates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Templates];
GO
IF OBJECT_ID(N'[dbo].[TemplateTypeBlocks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TemplateTypeBlocks];
GO
IF OBJECT_ID(N'[dbo].[TemplateTypeFields]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TemplateTypeFields];
GO
IF OBJECT_ID(N'[dbo].[TemplateTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TemplateTypes];
GO
IF OBJECT_ID(N'[dbo].[TemplateVariables]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TemplateVariables];
GO
IF OBJECT_ID(N'[dbo].[UserProfile]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile];
GO
IF OBJECT_ID(N'[dbo].[UserProfile_Fields]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile_Fields];
GO
IF OBJECT_ID(N'[dbo].[UserProfile_InstitutionLevels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile_InstitutionLevels];
GO
IF OBJECT_ID(N'[dbo].[UserProfile_Provinces]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile_Provinces];
GO
IF OBJECT_ID(N'[dbo].[webpages_Membership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Membership];
GO
IF OBJECT_ID(N'[dbo].[webpages_OAuthMembership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_OAuthMembership];
GO
IF OBJECT_ID(N'[dbo].[webpages_Permissions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Permissions];
GO
IF OBJECT_ID(N'[dbo].[webpages_Permissions_StatusCriterions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Permissions_StatusCriterions];
GO
IF OBJECT_ID(N'[dbo].[webpages_RolePermissionStatuses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_RolePermissionStatuses];
GO
IF OBJECT_ID(N'[dbo].[webpages_Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Roles];
GO
IF OBJECT_ID(N'[dbo].[webpages_UsersInRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_UsersInRoles];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Audits'
CREATE TABLE [dbo].[Audits] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int  NOT NULL,
    [EntityId] int  NOT NULL,
    [EntityName] nvarchar(50)  NOT NULL,
    [PropertyName] nvarchar(50)  NOT NULL,
    [AdditionalData] nvarchar(50)  NULL,
    [Action] nvarchar(50)  NOT NULL,
    [TimeStamp] datetime  NOT NULL,
    [OriginalValue] nvarchar(50)  NOT NULL,
    [CurrentValue] nvarchar(50)  NULL,
    [ImprovementPlanId] int  NULL
);
GO

-- Creating table 'Budgets'
CREATE TABLE [dbo].[Budgets] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SchoolYearId] int  NOT NULL,
    [ProvinceId] int  NOT NULL,
    [FieldId] int  NOT NULL,
    [LineId] int  NOT NULL,
    [Ammount] decimal(20,4)  NULL
);
GO

-- Creating table 'Comments'
CREATE TABLE [dbo].[Comments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int  NOT NULL,
    [Date] datetime  NOT NULL,
    [Text] nvarchar(4000)  NOT NULL,
    [IncludeInDictum] bit  NULL
);
GO

-- Creating table 'DictumTypes'
CREATE TABLE [dbo].[DictumTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'Documents'
CREATE TABLE [dbo].[Documents] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusId] int  NOT NULL,
    [TemplateId] int  NOT NULL,
    [EmissionDate] datetime  NULL,
    [SignatureDate] datetime  NULL,
    [Body] nvarchar(max)  NULL,
    [PDF] nvarchar(50)  NULL,
    [ImprovementPlanId] int  NULL,
    [CreationUserId] int  NOT NULL,
    [CreationDate] datetime  NOT NULL,
    [UserBody] nvarchar(max)  NULL,
    [SignedDocument] nvarchar(200)  NULL,
    [PDF_Annex] nvarchar(200)  NULL,
    [PDF_Annex_extra] nvarchar(200)  NULL
);
GO

-- Creating table 'ExpenditureTypes'
CREATE TABLE [dbo].[ExpenditureTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Fields'
CREATE TABLE [dbo].[Fields] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL,
    [Management] nvarchar(50)  NOT NULL,
    [Code] nvarchar(5)  NOT NULL,
    [StatusId] int  NOT NULL
);
GO

-- Creating table 'FileNumbers'
CREATE TABLE [dbo].[FileNumbers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LastFileNumber] nvarchar(6)  NOT NULL
);
GO

-- Creating table 'ImprovementPlans'
CREATE TABLE [dbo].[ImprovementPlans] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ImprovementPlanTypeId] int  NOT NULL,
    [FieldId] int  NOT NULL,
    [StatusId] int  NOT NULL,
    [SchoolYearId] int  NOT NULL,
    [ParentId] int  NULL,
    [Identifier] nvarchar(50)  NOT NULL,
    [CUE] nvarchar(50)  NOT NULL,
    [ReceptionDate] datetime  NOT NULL,
    [Summary] nvarchar(4000)  NOT NULL,
    [EvaluatorUserId] int  NULL,
    [FieldDate] datetime  NULL,
    [InstitutionName] nvarchar(250)  NULL,
    [Location] nvarchar(250)  NULL,
    [Department] nvarchar(250)  NULL,
    [Attachment] nvarchar(200)  NULL,
    [Documentation] nvarchar(200)  NULL,
    [InstitutionLevel] nvarchar(100)  NULL,
    [InstitutionLevelInt] int  NULL,
    [Dependence] nvarchar(100)  NULL
);
GO

-- Creating table 'ImprovementPlansTypes'
CREATE TABLE [dbo].[ImprovementPlansTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Incidences'
CREATE TABLE [dbo].[Incidences] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ImprovementPlanId] int  NOT NULL,
    [SolicitudeId] int  NULL,
    [UserId] int  NOT NULL,
    [IncidentTypeId] int  NOT NULL,
    [Date] datetime  NOT NULL,
    [Active] bit  NOT NULL,
    [Details] nvarchar(2000)  NOT NULL
);
GO

-- Creating table 'IncidenceTypes'
CREATE TABLE [dbo].[IncidenceTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'Institutions'
CREATE TABLE [dbo].[Institutions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Province] nvarchar(50)  NOT NULL,
    [Department] nvarchar(50)  NOT NULL,
    [Locality] nvarchar(50)  NOT NULL,
    [Scope] nvarchar(50)  NOT NULL,
    [Orientation] nvarchar(50)  NOT NULL,
    [InstitutionType] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Lines'
CREATE TABLE [dbo].[Lines] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FieldId] int  NOT NULL,
    [Description] nvarchar(250)  NOT NULL,
    [Code] nvarchar(1)  NOT NULL
);
GO

-- Creating table 'MeasurementUnits'
CREATE TABLE [dbo].[MeasurementUnits] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(20)  NOT NULL
);
GO

-- Creating table 'Provinces'
CREATE TABLE [dbo].[Provinces] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Number] nvarchar(2)  NOT NULL,
    [Code] nvarchar(4)  NOT NULL,
    [MinistryName] nvarchar(100)  NULL
);
GO

-- Creating table 'SchoolYears'
CREATE TABLE [dbo].[SchoolYears] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Cycle] nvarchar(50)  NOT NULL,
    [Description] nvarchar(100)  NOT NULL,
    [Active] bit  NULL
);
GO

-- Creating table 'Solicitudes'
CREATE TABLE [dbo].[Solicitudes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SchoolYearId] int  NOT NULL,
    [MeasurementUnitId] int  NOT NULL,
    [SolicitudeTypeId] int  NULL,
    [ExpenditureTypeId] int  NULL,
    [StatusId] int  NOT NULL,
    [ImprovementPlanId] int  NOT NULL,
    [LineId] int  NULL,
    [ReassignedId] int  NULL,
    [CUE] nvarchar(50)  NOT NULL,
    [Details] nvarchar(2000)  NOT NULL,
    [FileNumber] nvarchar(50)  NULL,
    [Specialization] nvarchar(1000)  NULL,
    [RequestedAmount] decimal(20,4)  NOT NULL,
    [RequestedPriceUnit] decimal(20,4)  NULL,
    [ApprovedAmount] decimal(20,4)  NULL,
    [ApprovedPriceUnit] decimal(20,4)  NULL,
    [Locked] bit  NOT NULL,
    [ReassignedRequestedTotal] decimal(20,4)  NULL,
    [ReassignedGrantedTotal] decimal(20,4)  NULL,
    [Level] nvarchar(1000)  NULL
);
GO

-- Creating table 'SolicitudeTypes'
CREATE TABLE [dbo].[SolicitudeTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'Status'
CREATE TABLE [dbo].[Status] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusCriterionId] int  NOT NULL,
    [Description] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'StatusCriterions'
CREATE TABLE [dbo].[StatusCriterions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'Templates'
CREATE TABLE [dbo].[Templates] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateTypeId] int  NOT NULL,
    [Name] nvarchar(200)  NOT NULL,
    [Active] bit  NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [marginLeft] float  NULL,
    [marginRight] float  NULL,
    [marginTop] float  NULL,
    [marginBottom] float  NULL,
    [Header] nvarchar(max)  NULL,
    [SignatureImagePath] nvarchar(200)  NULL
);
GO

-- Creating table 'TemplateTypeFields'
CREATE TABLE [dbo].[TemplateTypeFields] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateTypeId] int  NOT NULL,
    [Field] nvarchar(100)  NOT NULL,
    [Description] nvarchar(200)  NOT NULL
);
GO

-- Creating table 'TemplateTypes'
CREATE TABLE [dbo].[TemplateTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(60)  NOT NULL,
    [Eligibility] bit  NOT NULL
);
GO

-- Creating table 'UserProfiles'
CREATE TABLE [dbo].[UserProfiles] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [UserName] nvarchar(56)  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [LastName] nvarchar(50)  NOT NULL,
    [IsEnabled] bit  NOT NULL,
    [IsDeleted] bit  NOT NULL,
    [Province_Id] int  NULL
);
GO

-- Creating table 'webpages_Membership'
CREATE TABLE [dbo].[webpages_Membership] (
    [UserId] int  NOT NULL,
    [CreateDate] datetime  NULL,
    [ConfirmationToken] nvarchar(128)  NULL,
    [IsConfirmed] bit  NULL,
    [LastPasswordFailureDate] datetime  NULL,
    [PasswordFailuresSinceLastSuccess] int  NOT NULL,
    [Password] nvarchar(128)  NOT NULL,
    [PasswordChangedDate] datetime  NULL,
    [PasswordSalt] nvarchar(128)  NOT NULL,
    [PasswordVerificationToken] nvarchar(128)  NULL,
    [PasswordVerificationTokenExpirationDate] datetime  NULL
);
GO

-- Creating table 'webpages_OAuthMembership'
CREATE TABLE [dbo].[webpages_OAuthMembership] (
    [Provider] nvarchar(30)  NOT NULL,
    [ProviderUserId] nvarchar(100)  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'webpages_Roles'
CREATE TABLE [dbo].[webpages_Roles] (
    [RoleId] int IDENTITY(1,1) NOT NULL,
    [RoleName] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'TemplateTypeBlocks'
CREATE TABLE [dbo].[TemplateTypeBlocks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateTypeId] int  NOT NULL,
    [Block] nvarchar(100)  NOT NULL,
    [Description] nvarchar(200)  NOT NULL
);
GO

-- Creating table 'DocumentVariables'
CREATE TABLE [dbo].[DocumentVariables] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DocumentId] int  NOT NULL,
    [Text] nvarchar(500)  NOT NULL,
    [TemplateVariableId] int  NOT NULL
);
GO

-- Creating table 'TemplateVariables'
CREATE TABLE [dbo].[TemplateVariables] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateId] int  NOT NULL,
    [Variable] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'ELMAH_Error'
CREATE TABLE [dbo].[ELMAH_Error] (
    [ErrorId] uniqueidentifier  NOT NULL,
    [Application] nvarchar(60)  NOT NULL,
    [Host] nvarchar(50)  NOT NULL,
    [Type] nvarchar(100)  NOT NULL,
    [Source] nvarchar(60)  NOT NULL,
    [Message] nvarchar(500)  NOT NULL,
    [User] nvarchar(50)  NOT NULL,
    [StatusCode] int  NOT NULL,
    [TimeUtc] datetime  NOT NULL,
    [Sequence] int IDENTITY(1,1) NOT NULL,
    [AllXml] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'InstitutionLevels'
CREATE TABLE [dbo].[InstitutionLevels] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100)  NOT NULL,
    [Code] int  NOT NULL
);
GO

-- Creating table 'webpages_Permissions'
CREATE TABLE [dbo].[webpages_Permissions] (
    [PermissionId] int IDENTITY(1,1) NOT NULL,
    [PermissionName] nvarchar(256)  NOT NULL,
    [PermissionCode] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'webpages_RolePermissionStatuses'
CREATE TABLE [dbo].[webpages_RolePermissionStatuses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PermissionId] int  NOT NULL,
    [RoleId] int  NOT NULL,
    [StatusId] int  NULL
);
GO

-- Creating table 'Documents_Dictum'
CREATE TABLE [dbo].[Documents_Dictum] (
    [EligibilityStatusId] int  NULL,
    [Ammount] decimal(20,4)  NULL,
    [Balance] decimal(20,4)  NULL,
    [Number] int IDENTITY(1,1) NOT NULL,
    [EligibilityRequired] bit  NOT NULL,
    [EligibilityExternalNumber] nvarchar(50)  NULL,
    [ElegibilityExternalEntity] nvarchar(100)  NULL,
    [FileNumber] nvarchar(50)  NULL,
    [Locked] bit  NOT NULL,
    [DictumNumber] nvarchar(100)  NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'Documents_Resolution'
CREATE TABLE [dbo].[Documents_Resolution] (
    [ShipDate] datetime  NULL,
    [ProtocolizedDate] datetime  NULL,
    [AmountExecuted] decimal(20,4)  NULL,
    [Annex] nvarchar(200)  NULL,
    [Number] int IDENTITY(1,1) NOT NULL,
    [ResolutionNumber] nvarchar(100)  NULL,
    [AnnexSignatureDate] datetime  NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'Comments_ImprovementPlans'
CREATE TABLE [dbo].[Comments_ImprovementPlans] (
    [Comments_Id] int  NOT NULL,
    [ImprovementPlans_Id] int  NOT NULL
);
GO

-- Creating table 'Comments_Incidences'
CREATE TABLE [dbo].[Comments_Incidences] (
    [Comments_Id] int  NOT NULL,
    [Incidences_Id] int  NOT NULL
);
GO

-- Creating table 'Comments_Solicitudes'
CREATE TABLE [dbo].[Comments_Solicitudes] (
    [Comments_Id] int  NOT NULL,
    [Solicitudes_Id] int  NOT NULL
);
GO

-- Creating table 'Dictums_Solicitudes'
CREATE TABLE [dbo].[Dictums_Solicitudes] (
    [Dictums_Id] int  NOT NULL,
    [Solicitudes_Id] int  NOT NULL
);
GO

-- Creating table 'UserProfile_Fields'
CREATE TABLE [dbo].[UserProfile_Fields] (
    [Fields_Id] int  NOT NULL,
    [UserProfiles_UserId] int  NOT NULL
);
GO

-- Creating table 'webpages_UsersInRoles'
CREATE TABLE [dbo].[webpages_UsersInRoles] (
    [webpages_Roles_RoleId] int  NOT NULL,
    [UserProfiles_UserId] int  NOT NULL
);
GO

-- Creating table 'ResolutionDictums'
CREATE TABLE [dbo].[ResolutionDictums] (
    [Dictums_Id] int  NOT NULL,
    [Resolutions_Id] int  NOT NULL
);
GO

-- Creating table 'TemplateField'
CREATE TABLE [dbo].[TemplateField] (
    [Fields_Id] int  NOT NULL,
    [Templates_Id] int  NOT NULL
);
GO

-- Creating table 'UserProfile_Provinces'
CREATE TABLE [dbo].[UserProfile_Provinces] (
    [Provinces_Id] int  NOT NULL,
    [UserProfiles_UserId] int  NOT NULL
);
GO

-- Creating table 'UserProfile_InstitutionLevels'
CREATE TABLE [dbo].[UserProfile_InstitutionLevels] (
    [InstitutionLevels_Id] int  NOT NULL,
    [UserProfiles_UserId] int  NOT NULL
);
GO

-- Creating table 'webpages_Permissions_StatusCriterions'
CREATE TABLE [dbo].[webpages_Permissions_StatusCriterions] (
    [webpages_Permissions_PermissionId] int  NOT NULL,
    [StatusCriterions_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Audits'
ALTER TABLE [dbo].[Audits]
ADD CONSTRAINT [PK_Audits]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Budgets'
ALTER TABLE [dbo].[Budgets]
ADD CONSTRAINT [PK_Budgets]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [PK_Comments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DictumTypes'
ALTER TABLE [dbo].[DictumTypes]
ADD CONSTRAINT [PK_DictumTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [PK_Documents]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ExpenditureTypes'
ALTER TABLE [dbo].[ExpenditureTypes]
ADD CONSTRAINT [PK_ExpenditureTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Fields'
ALTER TABLE [dbo].[Fields]
ADD CONSTRAINT [PK_Fields]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FileNumbers'
ALTER TABLE [dbo].[FileNumbers]
ADD CONSTRAINT [PK_FileNumbers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [PK_ImprovementPlans]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ImprovementPlansTypes'
ALTER TABLE [dbo].[ImprovementPlansTypes]
ADD CONSTRAINT [PK_ImprovementPlansTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Incidences'
ALTER TABLE [dbo].[Incidences]
ADD CONSTRAINT [PK_Incidences]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'IncidenceTypes'
ALTER TABLE [dbo].[IncidenceTypes]
ADD CONSTRAINT [PK_IncidenceTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Institutions'
ALTER TABLE [dbo].[Institutions]
ADD CONSTRAINT [PK_Institutions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Lines'
ALTER TABLE [dbo].[Lines]
ADD CONSTRAINT [PK_Lines]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MeasurementUnits'
ALTER TABLE [dbo].[MeasurementUnits]
ADD CONSTRAINT [PK_MeasurementUnits]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Provinces'
ALTER TABLE [dbo].[Provinces]
ADD CONSTRAINT [PK_Provinces]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SchoolYears'
ALTER TABLE [dbo].[SchoolYears]
ADD CONSTRAINT [PK_SchoolYears]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [PK_Solicitudes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SolicitudeTypes'
ALTER TABLE [dbo].[SolicitudeTypes]
ADD CONSTRAINT [PK_SolicitudeTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Status'
ALTER TABLE [dbo].[Status]
ADD CONSTRAINT [PK_Status]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StatusCriterions'
ALTER TABLE [dbo].[StatusCriterions]
ADD CONSTRAINT [PK_StatusCriterions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Templates'
ALTER TABLE [dbo].[Templates]
ADD CONSTRAINT [PK_Templates]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TemplateTypeFields'
ALTER TABLE [dbo].[TemplateTypeFields]
ADD CONSTRAINT [PK_TemplateTypeFields]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TemplateTypes'
ALTER TABLE [dbo].[TemplateTypes]
ADD CONSTRAINT [PK_TemplateTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserId] in table 'UserProfiles'
ALTER TABLE [dbo].[UserProfiles]
ADD CONSTRAINT [PK_UserProfiles]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [UserId] in table 'webpages_Membership'
ALTER TABLE [dbo].[webpages_Membership]
ADD CONSTRAINT [PK_webpages_Membership]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [Provider], [ProviderUserId] in table 'webpages_OAuthMembership'
ALTER TABLE [dbo].[webpages_OAuthMembership]
ADD CONSTRAINT [PK_webpages_OAuthMembership]
    PRIMARY KEY CLUSTERED ([Provider], [ProviderUserId] ASC);
GO

-- Creating primary key on [RoleId] in table 'webpages_Roles'
ALTER TABLE [dbo].[webpages_Roles]
ADD CONSTRAINT [PK_webpages_Roles]
    PRIMARY KEY CLUSTERED ([RoleId] ASC);
GO

-- Creating primary key on [Id] in table 'TemplateTypeBlocks'
ALTER TABLE [dbo].[TemplateTypeBlocks]
ADD CONSTRAINT [PK_TemplateTypeBlocks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DocumentVariables'
ALTER TABLE [dbo].[DocumentVariables]
ADD CONSTRAINT [PK_DocumentVariables]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TemplateVariables'
ALTER TABLE [dbo].[TemplateVariables]
ADD CONSTRAINT [PK_TemplateVariables]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ErrorId] in table 'ELMAH_Error'
ALTER TABLE [dbo].[ELMAH_Error]
ADD CONSTRAINT [PK_ELMAH_Error]
    PRIMARY KEY CLUSTERED ([ErrorId] ASC);
GO

-- Creating primary key on [Id] in table 'InstitutionLevels'
ALTER TABLE [dbo].[InstitutionLevels]
ADD CONSTRAINT [PK_InstitutionLevels]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [PermissionId] in table 'webpages_Permissions'
ALTER TABLE [dbo].[webpages_Permissions]
ADD CONSTRAINT [PK_webpages_Permissions]
    PRIMARY KEY CLUSTERED ([PermissionId] ASC);
GO

-- Creating primary key on [Id] in table 'webpages_RolePermissionStatuses'
ALTER TABLE [dbo].[webpages_RolePermissionStatuses]
ADD CONSTRAINT [PK_webpages_RolePermissionStatuses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Documents_Dictum'
ALTER TABLE [dbo].[Documents_Dictum]
ADD CONSTRAINT [PK_Documents_Dictum]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Documents_Resolution'
ALTER TABLE [dbo].[Documents_Resolution]
ADD CONSTRAINT [PK_Documents_Resolution]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Comments_Id], [ImprovementPlans_Id] in table 'Comments_ImprovementPlans'
ALTER TABLE [dbo].[Comments_ImprovementPlans]
ADD CONSTRAINT [PK_Comments_ImprovementPlans]
    PRIMARY KEY CLUSTERED ([Comments_Id], [ImprovementPlans_Id] ASC);
GO

-- Creating primary key on [Comments_Id], [Incidences_Id] in table 'Comments_Incidences'
ALTER TABLE [dbo].[Comments_Incidences]
ADD CONSTRAINT [PK_Comments_Incidences]
    PRIMARY KEY CLUSTERED ([Comments_Id], [Incidences_Id] ASC);
GO

-- Creating primary key on [Comments_Id], [Solicitudes_Id] in table 'Comments_Solicitudes'
ALTER TABLE [dbo].[Comments_Solicitudes]
ADD CONSTRAINT [PK_Comments_Solicitudes]
    PRIMARY KEY CLUSTERED ([Comments_Id], [Solicitudes_Id] ASC);
GO

-- Creating primary key on [Dictums_Id], [Solicitudes_Id] in table 'Dictums_Solicitudes'
ALTER TABLE [dbo].[Dictums_Solicitudes]
ADD CONSTRAINT [PK_Dictums_Solicitudes]
    PRIMARY KEY CLUSTERED ([Dictums_Id], [Solicitudes_Id] ASC);
GO

-- Creating primary key on [Fields_Id], [UserProfiles_UserId] in table 'UserProfile_Fields'
ALTER TABLE [dbo].[UserProfile_Fields]
ADD CONSTRAINT [PK_UserProfile_Fields]
    PRIMARY KEY CLUSTERED ([Fields_Id], [UserProfiles_UserId] ASC);
GO

-- Creating primary key on [webpages_Roles_RoleId], [UserProfiles_UserId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [PK_webpages_UsersInRoles]
    PRIMARY KEY CLUSTERED ([webpages_Roles_RoleId], [UserProfiles_UserId] ASC);
GO

-- Creating primary key on [Dictums_Id], [Resolutions_Id] in table 'ResolutionDictums'
ALTER TABLE [dbo].[ResolutionDictums]
ADD CONSTRAINT [PK_ResolutionDictums]
    PRIMARY KEY CLUSTERED ([Dictums_Id], [Resolutions_Id] ASC);
GO

-- Creating primary key on [Fields_Id], [Templates_Id] in table 'TemplateField'
ALTER TABLE [dbo].[TemplateField]
ADD CONSTRAINT [PK_TemplateField]
    PRIMARY KEY CLUSTERED ([Fields_Id], [Templates_Id] ASC);
GO

-- Creating primary key on [Provinces_Id], [UserProfiles_UserId] in table 'UserProfile_Provinces'
ALTER TABLE [dbo].[UserProfile_Provinces]
ADD CONSTRAINT [PK_UserProfile_Provinces]
    PRIMARY KEY CLUSTERED ([Provinces_Id], [UserProfiles_UserId] ASC);
GO

-- Creating primary key on [InstitutionLevels_Id], [UserProfiles_UserId] in table 'UserProfile_InstitutionLevels'
ALTER TABLE [dbo].[UserProfile_InstitutionLevels]
ADD CONSTRAINT [PK_UserProfile_InstitutionLevels]
    PRIMARY KEY CLUSTERED ([InstitutionLevels_Id], [UserProfiles_UserId] ASC);
GO

-- Creating primary key on [webpages_Permissions_PermissionId], [StatusCriterions_Id] in table 'webpages_Permissions_StatusCriterions'
ALTER TABLE [dbo].[webpages_Permissions_StatusCriterions]
ADD CONSTRAINT [PK_webpages_Permissions_StatusCriterions]
    PRIMARY KEY CLUSTERED ([webpages_Permissions_PermissionId], [StatusCriterions_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [UserId] in table 'Audits'
ALTER TABLE [dbo].[Audits]
ADD CONSTRAINT [FK_Audits_UserProfile]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Audits_UserProfile'
CREATE INDEX [IX_FK_Audits_UserProfile]
ON [dbo].[Audits]
    ([UserId]);
GO

-- Creating foreign key on [FieldId] in table 'Budgets'
ALTER TABLE [dbo].[Budgets]
ADD CONSTRAINT [FK_Budgets_Fields]
    FOREIGN KEY ([FieldId])
    REFERENCES [dbo].[Fields]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Budgets_Fields'
CREATE INDEX [IX_FK_Budgets_Fields]
ON [dbo].[Budgets]
    ([FieldId]);
GO

-- Creating foreign key on [LineId] in table 'Budgets'
ALTER TABLE [dbo].[Budgets]
ADD CONSTRAINT [FK_Budgets_Lines]
    FOREIGN KEY ([LineId])
    REFERENCES [dbo].[Lines]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Budgets_Lines'
CREATE INDEX [IX_FK_Budgets_Lines]
ON [dbo].[Budgets]
    ([LineId]);
GO

-- Creating foreign key on [ProvinceId] in table 'Budgets'
ALTER TABLE [dbo].[Budgets]
ADD CONSTRAINT [FK_Budgets_Provinces]
    FOREIGN KEY ([ProvinceId])
    REFERENCES [dbo].[Provinces]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Budgets_Provinces'
CREATE INDEX [IX_FK_Budgets_Provinces]
ON [dbo].[Budgets]
    ([ProvinceId]);
GO

-- Creating foreign key on [SchoolYearId] in table 'Budgets'
ALTER TABLE [dbo].[Budgets]
ADD CONSTRAINT [FK_Budgets_SchoolYears]
    FOREIGN KEY ([SchoolYearId])
    REFERENCES [dbo].[SchoolYears]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Budgets_SchoolYears'
CREATE INDEX [IX_FK_Budgets_SchoolYears]
ON [dbo].[Budgets]
    ([SchoolYearId]);
GO

-- Creating foreign key on [UserId] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [FK_Comments_UserProfile]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comments_UserProfile'
CREATE INDEX [IX_FK_Comments_UserProfile]
ON [dbo].[Comments]
    ([UserId]);
GO

-- Creating foreign key on [EligibilityStatusId] in table 'Documents_Dictum'
ALTER TABLE [dbo].[Documents_Dictum]
ADD CONSTRAINT [FK_Dictums_Status]
    FOREIGN KEY ([EligibilityStatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Dictums_Status'
CREATE INDEX [IX_FK_Dictums_Status]
ON [dbo].[Documents_Dictum]
    ([EligibilityStatusId]);
GO

-- Creating foreign key on [StatusId] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [FK_DocumentsManagement_Status]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentsManagement_Status'
CREATE INDEX [IX_FK_DocumentsManagement_Status]
ON [dbo].[Documents]
    ([StatusId]);
GO

-- Creating foreign key on [TemplateId] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [FK_DocumentsManagement_Templates]
    FOREIGN KEY ([TemplateId])
    REFERENCES [dbo].[Templates]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentsManagement_Templates'
CREATE INDEX [IX_FK_DocumentsManagement_Templates]
ON [dbo].[Documents]
    ([TemplateId]);
GO

-- Creating foreign key on [ExpenditureTypeId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_ExpenditureTypes]
    FOREIGN KEY ([ExpenditureTypeId])
    REFERENCES [dbo].[ExpenditureTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_ExpenditureTypes'
CREATE INDEX [IX_FK_Solicitudes_ExpenditureTypes]
ON [dbo].[Solicitudes]
    ([ExpenditureTypeId]);
GO

-- Creating foreign key on [FieldId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_Fields]
    FOREIGN KEY ([FieldId])
    REFERENCES [dbo].[Fields]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_Fields'
CREATE INDEX [IX_FK_ImprovementPlans_Fields]
ON [dbo].[ImprovementPlans]
    ([FieldId]);
GO

-- Creating foreign key on [FieldId] in table 'Lines'
ALTER TABLE [dbo].[Lines]
ADD CONSTRAINT [FK_Lines_Fields]
    FOREIGN KEY ([FieldId])
    REFERENCES [dbo].[Fields]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Lines_Fields'
CREATE INDEX [IX_FK_Lines_Fields]
ON [dbo].[Lines]
    ([FieldId]);
GO

-- Creating foreign key on [ParentId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_ImprovementPlans]
    FOREIGN KEY ([ParentId])
    REFERENCES [dbo].[ImprovementPlans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_ImprovementPlans'
CREATE INDEX [IX_FK_ImprovementPlans_ImprovementPlans]
ON [dbo].[ImprovementPlans]
    ([ParentId]);
GO

-- Creating foreign key on [ImprovementPlanTypeId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_ImprovementPlansTypes]
    FOREIGN KEY ([ImprovementPlanTypeId])
    REFERENCES [dbo].[ImprovementPlansTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_ImprovementPlansTypes'
CREATE INDEX [IX_FK_ImprovementPlans_ImprovementPlansTypes]
ON [dbo].[ImprovementPlans]
    ([ImprovementPlanTypeId]);
GO

-- Creating foreign key on [SchoolYearId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_SchoolYears]
    FOREIGN KEY ([SchoolYearId])
    REFERENCES [dbo].[SchoolYears]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_SchoolYears'
CREATE INDEX [IX_FK_ImprovementPlans_SchoolYears]
ON [dbo].[ImprovementPlans]
    ([SchoolYearId]);
GO

-- Creating foreign key on [StatusId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_Status]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_Status'
CREATE INDEX [IX_FK_ImprovementPlans_Status]
ON [dbo].[ImprovementPlans]
    ([StatusId]);
GO

-- Creating foreign key on [EvaluatorUserId] in table 'ImprovementPlans'
ALTER TABLE [dbo].[ImprovementPlans]
ADD CONSTRAINT [FK_ImprovementPlans_UserProfile]
    FOREIGN KEY ([EvaluatorUserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ImprovementPlans_UserProfile'
CREATE INDEX [IX_FK_ImprovementPlans_UserProfile]
ON [dbo].[ImprovementPlans]
    ([EvaluatorUserId]);
GO

-- Creating foreign key on [ImprovementPlanId] in table 'Incidences'
ALTER TABLE [dbo].[Incidences]
ADD CONSTRAINT [FK_Incidences_ImprovementPlans]
    FOREIGN KEY ([ImprovementPlanId])
    REFERENCES [dbo].[ImprovementPlans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Incidences_ImprovementPlans'
CREATE INDEX [IX_FK_Incidences_ImprovementPlans]
ON [dbo].[Incidences]
    ([ImprovementPlanId]);
GO

-- Creating foreign key on [ImprovementPlanId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_ImprovementPlans]
    FOREIGN KEY ([ImprovementPlanId])
    REFERENCES [dbo].[ImprovementPlans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_ImprovementPlans'
CREATE INDEX [IX_FK_Solicitudes_ImprovementPlans]
ON [dbo].[Solicitudes]
    ([ImprovementPlanId]);
GO

-- Creating foreign key on [IncidentTypeId] in table 'Incidences'
ALTER TABLE [dbo].[Incidences]
ADD CONSTRAINT [FK_Incidences_IncidenceTypes]
    FOREIGN KEY ([IncidentTypeId])
    REFERENCES [dbo].[IncidenceTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Incidences_IncidenceTypes'
CREATE INDEX [IX_FK_Incidences_IncidenceTypes]
ON [dbo].[Incidences]
    ([IncidentTypeId]);
GO

-- Creating foreign key on [SolicitudeId] in table 'Incidences'
ALTER TABLE [dbo].[Incidences]
ADD CONSTRAINT [FK_Incidences_Solicitudes]
    FOREIGN KEY ([SolicitudeId])
    REFERENCES [dbo].[Solicitudes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Incidences_Solicitudes'
CREATE INDEX [IX_FK_Incidences_Solicitudes]
ON [dbo].[Incidences]
    ([SolicitudeId]);
GO

-- Creating foreign key on [UserId] in table 'Incidences'
ALTER TABLE [dbo].[Incidences]
ADD CONSTRAINT [FK_Incidences_UserProfile]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Incidences_UserProfile'
CREATE INDEX [IX_FK_Incidences_UserProfile]
ON [dbo].[Incidences]
    ([UserId]);
GO

-- Creating foreign key on [LineId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_Lines]
    FOREIGN KEY ([LineId])
    REFERENCES [dbo].[Lines]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_Lines'
CREATE INDEX [IX_FK_Solicitudes_Lines]
ON [dbo].[Solicitudes]
    ([LineId]);
GO

-- Creating foreign key on [MeasurementUnitId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_MeasurementUnits]
    FOREIGN KEY ([MeasurementUnitId])
    REFERENCES [dbo].[MeasurementUnits]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_MeasurementUnits'
CREATE INDEX [IX_FK_Solicitudes_MeasurementUnits]
ON [dbo].[Solicitudes]
    ([MeasurementUnitId]);
GO

-- Creating foreign key on [SchoolYearId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_SchoolYears]
    FOREIGN KEY ([SchoolYearId])
    REFERENCES [dbo].[SchoolYears]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_SchoolYears'
CREATE INDEX [IX_FK_Solicitudes_SchoolYears]
ON [dbo].[Solicitudes]
    ([SchoolYearId]);
GO

-- Creating foreign key on [SolicitudeTypeId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_SolicitudeTypes]
    FOREIGN KEY ([SolicitudeTypeId])
    REFERENCES [dbo].[SolicitudeTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_SolicitudeTypes'
CREATE INDEX [IX_FK_Solicitudes_SolicitudeTypes]
ON [dbo].[Solicitudes]
    ([SolicitudeTypeId]);
GO

-- Creating foreign key on [StatusId] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_Status]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Solicitudes_Status'
CREATE INDEX [IX_FK_Solicitudes_Status]
ON [dbo].[Solicitudes]
    ([StatusId]);
GO

-- Creating foreign key on [StatusCriterionId] in table 'Status'
ALTER TABLE [dbo].[Status]
ADD CONSTRAINT [FK_Status_StatusCriterions]
    FOREIGN KEY ([StatusCriterionId])
    REFERENCES [dbo].[StatusCriterions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Status_StatusCriterions'
CREATE INDEX [IX_FK_Status_StatusCriterions]
ON [dbo].[Status]
    ([StatusCriterionId]);
GO

-- Creating foreign key on [TemplateTypeId] in table 'Templates'
ALTER TABLE [dbo].[Templates]
ADD CONSTRAINT [FK_Templates_TemplateTypes]
    FOREIGN KEY ([TemplateTypeId])
    REFERENCES [dbo].[TemplateTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Templates_TemplateTypes'
CREATE INDEX [IX_FK_Templates_TemplateTypes]
ON [dbo].[Templates]
    ([TemplateTypeId]);
GO

-- Creating foreign key on [TemplateTypeId] in table 'TemplateTypeFields'
ALTER TABLE [dbo].[TemplateTypeFields]
ADD CONSTRAINT [FK_TemplateTypeFields_TemplateTypes]
    FOREIGN KEY ([TemplateTypeId])
    REFERENCES [dbo].[TemplateTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateTypeFields_TemplateTypes'
CREATE INDEX [IX_FK_TemplateTypeFields_TemplateTypes]
ON [dbo].[TemplateTypeFields]
    ([TemplateTypeId]);
GO

-- Creating foreign key on [Comments_Id] in table 'Comments_ImprovementPlans'
ALTER TABLE [dbo].[Comments_ImprovementPlans]
ADD CONSTRAINT [FK_Comments_ImprovementPlans_Comments]
    FOREIGN KEY ([Comments_Id])
    REFERENCES [dbo].[Comments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ImprovementPlans_Id] in table 'Comments_ImprovementPlans'
ALTER TABLE [dbo].[Comments_ImprovementPlans]
ADD CONSTRAINT [FK_Comments_ImprovementPlans_ImprovementPlans]
    FOREIGN KEY ([ImprovementPlans_Id])
    REFERENCES [dbo].[ImprovementPlans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comments_ImprovementPlans_ImprovementPlans'
CREATE INDEX [IX_FK_Comments_ImprovementPlans_ImprovementPlans]
ON [dbo].[Comments_ImprovementPlans]
    ([ImprovementPlans_Id]);
GO

-- Creating foreign key on [Comments_Id] in table 'Comments_Incidences'
ALTER TABLE [dbo].[Comments_Incidences]
ADD CONSTRAINT [FK_Comments_Incidences_Comments]
    FOREIGN KEY ([Comments_Id])
    REFERENCES [dbo].[Comments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Incidences_Id] in table 'Comments_Incidences'
ALTER TABLE [dbo].[Comments_Incidences]
ADD CONSTRAINT [FK_Comments_Incidences_Incidences]
    FOREIGN KEY ([Incidences_Id])
    REFERENCES [dbo].[Incidences]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comments_Incidences_Incidences'
CREATE INDEX [IX_FK_Comments_Incidences_Incidences]
ON [dbo].[Comments_Incidences]
    ([Incidences_Id]);
GO

-- Creating foreign key on [Comments_Id] in table 'Comments_Solicitudes'
ALTER TABLE [dbo].[Comments_Solicitudes]
ADD CONSTRAINT [FK_Comments_Solicitudes_Comments]
    FOREIGN KEY ([Comments_Id])
    REFERENCES [dbo].[Comments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Solicitudes_Id] in table 'Comments_Solicitudes'
ALTER TABLE [dbo].[Comments_Solicitudes]
ADD CONSTRAINT [FK_Comments_Solicitudes_Solicitudes]
    FOREIGN KEY ([Solicitudes_Id])
    REFERENCES [dbo].[Solicitudes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comments_Solicitudes_Solicitudes'
CREATE INDEX [IX_FK_Comments_Solicitudes_Solicitudes]
ON [dbo].[Comments_Solicitudes]
    ([Solicitudes_Id]);
GO

-- Creating foreign key on [Dictums_Id] in table 'Dictums_Solicitudes'
ALTER TABLE [dbo].[Dictums_Solicitudes]
ADD CONSTRAINT [FK_Dictums_Solicitudes_Dictums]
    FOREIGN KEY ([Dictums_Id])
    REFERENCES [dbo].[Documents_Dictum]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Solicitudes_Id] in table 'Dictums_Solicitudes'
ALTER TABLE [dbo].[Dictums_Solicitudes]
ADD CONSTRAINT [FK_Dictums_Solicitudes_Solicitudes]
    FOREIGN KEY ([Solicitudes_Id])
    REFERENCES [dbo].[Solicitudes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Dictums_Solicitudes_Solicitudes'
CREATE INDEX [IX_FK_Dictums_Solicitudes_Solicitudes]
ON [dbo].[Dictums_Solicitudes]
    ([Solicitudes_Id]);
GO

-- Creating foreign key on [Fields_Id] in table 'UserProfile_Fields'
ALTER TABLE [dbo].[UserProfile_Fields]
ADD CONSTRAINT [FK_UserProfile_Fields_Fields]
    FOREIGN KEY ([Fields_Id])
    REFERENCES [dbo].[Fields]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfiles_UserId] in table 'UserProfile_Fields'
ALTER TABLE [dbo].[UserProfile_Fields]
ADD CONSTRAINT [FK_UserProfile_Fields_UserProfile]
    FOREIGN KEY ([UserProfiles_UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfile_Fields_UserProfile'
CREATE INDEX [IX_FK_UserProfile_Fields_UserProfile]
ON [dbo].[UserProfile_Fields]
    ([UserProfiles_UserId]);
GO

-- Creating foreign key on [webpages_Roles_RoleId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [FK_webpages_UsersInRoles_webpages_Roles]
    FOREIGN KEY ([webpages_Roles_RoleId])
    REFERENCES [dbo].[webpages_Roles]
        ([RoleId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfiles_UserId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [FK_webpages_UsersInRoles_UserProfile]
    FOREIGN KEY ([UserProfiles_UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_webpages_UsersInRoles_UserProfile'
CREATE INDEX [IX_FK_webpages_UsersInRoles_UserProfile]
ON [dbo].[webpages_UsersInRoles]
    ([UserProfiles_UserId]);
GO

-- Creating foreign key on [Id] in table 'Solicitudes'
ALTER TABLE [dbo].[Solicitudes]
ADD CONSTRAINT [FK_Solicitudes_Solicitudes1]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Solicitudes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ImprovementPlanId] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [FK_Documents_ImprovementPlans]
    FOREIGN KEY ([ImprovementPlanId])
    REFERENCES [dbo].[ImprovementPlans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Documents_ImprovementPlans'
CREATE INDEX [IX_FK_Documents_ImprovementPlans]
ON [dbo].[Documents]
    ([ImprovementPlanId]);
GO

-- Creating foreign key on [CreationUserId] in table 'Documents'
ALTER TABLE [dbo].[Documents]
ADD CONSTRAINT [FK_Documents_UserProfile]
    FOREIGN KEY ([CreationUserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Documents_UserProfile'
CREATE INDEX [IX_FK_Documents_UserProfile]
ON [dbo].[Documents]
    ([CreationUserId]);
GO

-- Creating foreign key on [Dictums_Id] in table 'ResolutionDictums'
ALTER TABLE [dbo].[ResolutionDictums]
ADD CONSTRAINT [FK_ResolutionDictums_Dictum]
    FOREIGN KEY ([Dictums_Id])
    REFERENCES [dbo].[Documents_Dictum]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Resolutions_Id] in table 'ResolutionDictums'
ALTER TABLE [dbo].[ResolutionDictums]
ADD CONSTRAINT [FK_ResolutionDictums_Resolution]
    FOREIGN KEY ([Resolutions_Id])
    REFERENCES [dbo].[Documents_Resolution]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ResolutionDictums_Resolution'
CREATE INDEX [IX_FK_ResolutionDictums_Resolution]
ON [dbo].[ResolutionDictums]
    ([Resolutions_Id]);
GO

-- Creating foreign key on [TemplateTypeId] in table 'TemplateTypeBlocks'
ALTER TABLE [dbo].[TemplateTypeBlocks]
ADD CONSTRAINT [FK_TemplateTypeBlocks_TemplateTypes]
    FOREIGN KEY ([TemplateTypeId])
    REFERENCES [dbo].[TemplateTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateTypeBlocks_TemplateTypes'
CREATE INDEX [IX_FK_TemplateTypeBlocks_TemplateTypes]
ON [dbo].[TemplateTypeBlocks]
    ([TemplateTypeId]);
GO

-- Creating foreign key on [DocumentId] in table 'DocumentVariables'
ALTER TABLE [dbo].[DocumentVariables]
ADD CONSTRAINT [FK_DocumentVariables_Documents]
    FOREIGN KEY ([DocumentId])
    REFERENCES [dbo].[Documents]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentVariables_Documents'
CREATE INDEX [IX_FK_DocumentVariables_Documents]
ON [dbo].[DocumentVariables]
    ([DocumentId]);
GO

-- Creating foreign key on [TemplateVariableId] in table 'DocumentVariables'
ALTER TABLE [dbo].[DocumentVariables]
ADD CONSTRAINT [FK_DocumentVariables_TemplateTypeVariables1]
    FOREIGN KEY ([TemplateVariableId])
    REFERENCES [dbo].[TemplateVariables]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentVariables_TemplateTypeVariables1'
CREATE INDEX [IX_FK_DocumentVariables_TemplateTypeVariables1]
ON [dbo].[DocumentVariables]
    ([TemplateVariableId]);
GO

-- Creating foreign key on [TemplateId] in table 'TemplateVariables'
ALTER TABLE [dbo].[TemplateVariables]
ADD CONSTRAINT [FK_TemplateTypeVariables_Templates]
    FOREIGN KEY ([TemplateId])
    REFERENCES [dbo].[Templates]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateTypeVariables_Templates'
CREATE INDEX [IX_FK_TemplateTypeVariables_Templates]
ON [dbo].[TemplateVariables]
    ([TemplateId]);
GO

-- Creating foreign key on [Fields_Id] in table 'TemplateField'
ALTER TABLE [dbo].[TemplateField]
ADD CONSTRAINT [FK_TemplateField_Field]
    FOREIGN KEY ([Fields_Id])
    REFERENCES [dbo].[Fields]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Templates_Id] in table 'TemplateField'
ALTER TABLE [dbo].[TemplateField]
ADD CONSTRAINT [FK_TemplateField_Template]
    FOREIGN KEY ([Templates_Id])
    REFERENCES [dbo].[Templates]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateField_Template'
CREATE INDEX [IX_FK_TemplateField_Template]
ON [dbo].[TemplateField]
    ([Templates_Id]);
GO

-- Creating foreign key on [Provinces_Id] in table 'UserProfile_Provinces'
ALTER TABLE [dbo].[UserProfile_Provinces]
ADD CONSTRAINT [FK_UserProfile_Provinces_Province]
    FOREIGN KEY ([Provinces_Id])
    REFERENCES [dbo].[Provinces]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfiles_UserId] in table 'UserProfile_Provinces'
ALTER TABLE [dbo].[UserProfile_Provinces]
ADD CONSTRAINT [FK_UserProfile_Provinces_UserProfile]
    FOREIGN KEY ([UserProfiles_UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfile_Provinces_UserProfile'
CREATE INDEX [IX_FK_UserProfile_Provinces_UserProfile]
ON [dbo].[UserProfile_Provinces]
    ([UserProfiles_UserId]);
GO

-- Creating foreign key on [StatusId] in table 'Fields'
ALTER TABLE [dbo].[Fields]
ADD CONSTRAINT [FK_FieldStatus]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FieldStatus'
CREATE INDEX [IX_FK_FieldStatus]
ON [dbo].[Fields]
    ([StatusId]);
GO

-- Creating foreign key on [InstitutionLevels_Id] in table 'UserProfile_InstitutionLevels'
ALTER TABLE [dbo].[UserProfile_InstitutionLevels]
ADD CONSTRAINT [FK_UserProfile_InstitutionLevels_InstitutionLevel]
    FOREIGN KEY ([InstitutionLevels_Id])
    REFERENCES [dbo].[InstitutionLevels]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfiles_UserId] in table 'UserProfile_InstitutionLevels'
ALTER TABLE [dbo].[UserProfile_InstitutionLevels]
ADD CONSTRAINT [FK_UserProfile_InstitutionLevels_UserProfile]
    FOREIGN KEY ([UserProfiles_UserId])
    REFERENCES [dbo].[UserProfiles]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfile_InstitutionLevels_UserProfile'
CREATE INDEX [IX_FK_UserProfile_InstitutionLevels_UserProfile]
ON [dbo].[UserProfile_InstitutionLevels]
    ([UserProfiles_UserId]);
GO

-- Creating foreign key on [webpages_Permissions_PermissionId] in table 'webpages_Permissions_StatusCriterions'
ALTER TABLE [dbo].[webpages_Permissions_StatusCriterions]
ADD CONSTRAINT [FK_webpages_Permissions_StatusCriterions_webpages_Permissions]
    FOREIGN KEY ([webpages_Permissions_PermissionId])
    REFERENCES [dbo].[webpages_Permissions]
        ([PermissionId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [StatusCriterions_Id] in table 'webpages_Permissions_StatusCriterions'
ALTER TABLE [dbo].[webpages_Permissions_StatusCriterions]
ADD CONSTRAINT [FK_webpages_Permissions_StatusCriterions_StatusCriterion]
    FOREIGN KEY ([StatusCriterions_Id])
    REFERENCES [dbo].[StatusCriterions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_webpages_Permissions_StatusCriterions_StatusCriterion'
CREATE INDEX [IX_FK_webpages_Permissions_StatusCriterions_StatusCriterion]
ON [dbo].[webpages_Permissions_StatusCriterions]
    ([StatusCriterions_Id]);
GO

-- Creating foreign key on [StatusId] in table 'webpages_RolePermissionStatuses'
ALTER TABLE [dbo].[webpages_RolePermissionStatuses]
ADD CONSTRAINT [FK_webpages_RolePermissionStatuses_Status]
    FOREIGN KEY ([StatusId])
    REFERENCES [dbo].[Status]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_webpages_RolePermissionStatuses_Status'
CREATE INDEX [IX_FK_webpages_RolePermissionStatuses_Status]
ON [dbo].[webpages_RolePermissionStatuses]
    ([StatusId]);
GO

-- Creating foreign key on [PermissionId] in table 'webpages_RolePermissionStatuses'
ALTER TABLE [dbo].[webpages_RolePermissionStatuses]
ADD CONSTRAINT [FK_webpages_RolePermissionStatuses_webpages_Permissions]
    FOREIGN KEY ([PermissionId])
    REFERENCES [dbo].[webpages_Permissions]
        ([PermissionId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_webpages_RolePermissionStatuses_webpages_Permissions'
CREATE INDEX [IX_FK_webpages_RolePermissionStatuses_webpages_Permissions]
ON [dbo].[webpages_RolePermissionStatuses]
    ([PermissionId]);
GO

-- Creating foreign key on [RoleId] in table 'webpages_RolePermissionStatuses'
ALTER TABLE [dbo].[webpages_RolePermissionStatuses]
ADD CONSTRAINT [FK_RolePermissionStatuses_webpages_Roles]
    FOREIGN KEY ([RoleId])
    REFERENCES [dbo].[webpages_Roles]
        ([RoleId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RolePermissionStatuses_webpages_Roles'
CREATE INDEX [IX_FK_RolePermissionStatuses_webpages_Roles]
ON [dbo].[webpages_RolePermissionStatuses]
    ([RoleId]);
GO

-- Creating foreign key on [Id] in table 'Documents_Dictum'
ALTER TABLE [dbo].[Documents_Dictum]
ADD CONSTRAINT [FK_Dictum_inherits_Document]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Documents]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Documents_Resolution'
ALTER TABLE [dbo].[Documents_Resolution]
ADD CONSTRAINT [FK_Resolution_inherits_Document]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Documents]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------