-- PolyMon Database Initialization Script
-- Run against an empty SQL Server database before first launch.
-- Idempotent: uses IF NOT EXISTS checks throughout.

SET NOCOUNT ON;
GO

-- ============================================================
-- ASP.NET Core Identity tables
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id]               NVARCHAR(450) NOT NULL,
        [Name]             NVARCHAR(256) NULL,
        [NormalizedName]   NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id]                   NVARCHAR(450)  NOT NULL,
        [DisplayName]          NVARCHAR(MAX)  NULL,
        [UserName]             NVARCHAR(256)  NULL,
        [NormalizedUserName]   NVARCHAR(256)  NULL,
        [Email]                NVARCHAR(256)  NULL,
        [NormalizedEmail]      NVARCHAR(256)  NULL,
        [EmailConfirmed]       BIT            NOT NULL,
        [PasswordHash]         NVARCHAR(MAX)  NULL,
        [SecurityStamp]        NVARCHAR(MAX)  NULL,
        [ConcurrencyStamp]     NVARCHAR(MAX)  NULL,
        [PhoneNumber]          NVARCHAR(MAX)  NULL,
        [PhoneNumberConfirmed] BIT            NOT NULL,
        [TwoFactorEnabled]     BIT            NOT NULL,
        [LockoutEnd]           DATETIMEOFFSET NULL,
        [LockoutEnabled]       BIT            NOT NULL,
        [AccessFailedCount]    INT            NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [UserNameIndex]  ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    CREATE        INDEX [EmailIndex]     ON [dbo].[AspNetUsers] ([NormalizedEmail])    WHERE [NormalizedEmail]    IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id]         INT           IDENTITY(1,1) NOT NULL,
        [RoleId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id]         INT           IDENTITY(1,1) NOT NULL,
        [UserId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(128) NOT NULL,
        [ProviderKey]         NVARCHAR(128) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId]              NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]  ([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId]        NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(128) NOT NULL,
        [Name]          NVARCHAR(128) NOT NULL,
        [Value]         NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

-- ============================================================
-- MonitorType  (plugin registry, populated by SyncPluginsAsync)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MonitorType')
BEGIN
    CREATE TABLE [dbo].[MonitorType] (
        [MonitorTypeID]   INT           IDENTITY(1,1) NOT NULL,
        [TypeKey]         NVARCHAR(50)  NOT NULL,
        [Name]            NVARCHAR(100) NOT NULL,
        [Description]     NVARCHAR(500) NULL,
        [IsEnabled]       BIT           NOT NULL CONSTRAINT [DF_MonitorType_IsEnabled] DEFAULT (1),
        CONSTRAINT [PK_MonitorType] PRIMARY KEY CLUSTERED ([MonitorTypeID] ASC)
    );
    CREATE UNIQUE INDEX [UX_MonitorType_TypeKey] ON [dbo].[MonitorType] ([TypeKey] ASC);
END
GO

-- ============================================================
-- Monitor  (flattened: core + alert rules + retention + after-event)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Monitor')
BEGIN
    CREATE TABLE [dbo].[Monitor] (
        -- Core
        [MonitorID]                     INT           IDENTITY(1,1) NOT NULL,
        [Name]                          NVARCHAR(50)  NOT NULL,
        [IsEnabled]                     BIT           NOT NULL CONSTRAINT [DF_Monitor_IsEnabled]         DEFAULT (1),
        [MonitorTypeID]                 INT           NOT NULL,
        [MonitorXML]                    NTEXT         NOT NULL CONSTRAINT [DF_Monitor_MonitorXML]        DEFAULT (''),
        [TriggerMod]                    INT           NOT NULL CONSTRAINT [DF_Monitor_TriggerMod]        DEFAULT (5),
        -- Offline suppression windows
        [OfflineTime1Start]             CHAR(5)       NOT NULL CONSTRAINT [DF_Monitor_OT1Start]          DEFAULT ('00:00'),
        [OfflineTime1End]               CHAR(5)       NOT NULL CONSTRAINT [DF_Monitor_OT1End]            DEFAULT ('00:00'),
        [OfflineTime2Start]             CHAR(5)       NOT NULL CONSTRAINT [DF_Monitor_OT2Start]          DEFAULT ('00:00'),
        [OfflineTime2End]               CHAR(5)       NOT NULL CONSTRAINT [DF_Monitor_OT2End]            DEFAULT ('00:00'),
        -- Email templates
        [MessageSubjectTemplate]        NVARCHAR(100) NOT NULL CONSTRAINT [DF_Monitor_SubjectTpl]        DEFAULT (''),
        [MessageBodyTemplate]           NVARCHAR(3000) NULL,
        -- Alert rules (flattened from MonitorAlertRule)
        [AlertAfterEveryNEvent]         INT           NOT NULL CONSTRAINT [DF_Monitor_AlertNEvent]       DEFAULT (0),
        [AlertAfterEveryNewFailure]     BIT           NOT NULL CONSTRAINT [DF_Monitor_AlertNewFail]      DEFAULT (1),
        [AlertAfterEveryNFailures]      INT           NOT NULL CONSTRAINT [DF_Monitor_AlertNFail]        DEFAULT (0),
        [AlertAfterEveryFailToOK]       BIT           NOT NULL CONSTRAINT [DF_Monitor_AlertFailOk]       DEFAULT (1),
        [AlertAfterEveryNewWarning]     BIT           NOT NULL CONSTRAINT [DF_Monitor_AlertNewWarn]      DEFAULT (0),
        [AlertAfterEveryNWarnings]      INT           NOT NULL CONSTRAINT [DF_Monitor_AlertNWarn]        DEFAULT (0),
        [AlertAfterEveryWarnToOK]       BIT           NOT NULL CONSTRAINT [DF_Monitor_AlertWarnOk]       DEFAULT (0),
        -- Retention (flattened from MonitorRetentionScheme)
        [MaxMonthsRaw]                  SMALLINT      NOT NULL CONSTRAINT [DF_Monitor_RetRaw]            DEFAULT (24),
        [MaxMonthsDaily]                SMALLINT      NOT NULL CONSTRAINT [DF_Monitor_RetDaily]          DEFAULT (36),
        [MaxMonthsWeekly]               SMALLINT      NOT NULL CONSTRAINT [DF_Monitor_RetWeekly]         DEFAULT (60),
        [MaxMonthsMonthly]              SMALLINT      NOT NULL CONSTRAINT [DF_Monitor_RetMonthly]        DEFAULT (60),
        -- After-event script (flattened from MonitorAction)
        [AfterEventIsEnabled]           BIT           NOT NULL CONSTRAINT [DF_Monitor_AfterEnabled]      DEFAULT (0),
        [AfterEventScriptEngineID]      INT           NOT NULL CONSTRAINT [DF_Monitor_AfterEngineID]     DEFAULT (0),
        [AfterEventScript]              NTEXT         NULL,
        -- Audit
        [AuditCreateDT]                 DATETIME      NOT NULL CONSTRAINT [DF_Monitor_AuditCreate]       DEFAULT (GETDATE()),
        [AuditUpdateDT]                 DATETIME      NULL,
        CONSTRAINT [PK_Monitor] PRIMARY KEY CLUSTERED ([MonitorID] ASC),
        CONSTRAINT [FK_Monitor_MonitorType] FOREIGN KEY ([MonitorTypeID]) REFERENCES [dbo].[MonitorType] ([MonitorTypeID])
    );
END
GO

-- ============================================================
-- MonitorEvent
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MonitorEvent')
BEGIN
    CREATE TABLE [dbo].[MonitorEvent] (
        [EventID]    INT            IDENTITY(1,1) NOT NULL,
        [MonitorID]  INT            NOT NULL,
        [EventDT]    DATETIME       NOT NULL,
        [StatusID]   TINYINT        NOT NULL,
        [Message]    NVARCHAR(500)  NULL,
        [AlertSent]  BIT            NOT NULL CONSTRAINT [DF_MonitorEvent_AlertSent] DEFAULT (0),
        CONSTRAINT [PK_MonitorEvent] PRIMARY KEY CLUSTERED ([EventID] ASC),
        CONSTRAINT [FK_MonitorEvent_Monitor] FOREIGN KEY ([MonitorID]) REFERENCES [dbo].[Monitor] ([MonitorID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_MonitorEvent_MonitorDT] ON [dbo].[MonitorEvent] ([MonitorID] ASC, [EventDT] ASC, [StatusID] ASC);
    CREATE NONCLUSTERED INDEX [IX_MonitorEvent_DT]        ON [dbo].[MonitorEvent] ([EventDT] ASC, [StatusID] ASC);
END
GO

-- ============================================================
-- MonitorEventCounter  (auto-increment PK to allow easy EF querying)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MonitorEventCounter')
BEGIN
    CREATE TABLE [dbo].[MonitorEventCounter] (
        [CounterID]    INT             IDENTITY(1,1) NOT NULL,
        [EventID]      INT             NOT NULL,
        [MonitorID]    INT             NOT NULL,
        [CounterName]  VARCHAR(255)    NOT NULL,
        [CounterValue] DECIMAL(30,10)  NULL,
        [EventDT]      DATETIME        NOT NULL,
        CONSTRAINT [PK_MonitorEventCounter] PRIMARY KEY CLUSTERED ([CounterID] ASC),
        CONSTRAINT [FK_MonitorEventCounter_Event] FOREIGN KEY ([EventID]) REFERENCES [dbo].[MonitorEvent] ([EventID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_MonitorEventCounter_MonitorDT] ON [dbo].[MonitorEventCounter] ([MonitorID] ASC, [EventDT] ASC);
END
GO

-- ============================================================
-- Operator
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Operator')
BEGIN
    CREATE TABLE [dbo].[Operator] (
        [OperatorID]            INT           IDENTITY(1,1) NOT NULL,
        [Name]                  NVARCHAR(255) NOT NULL,
        [IsEnabled]             BIT           NOT NULL CONSTRAINT [DF_Operator_IsEnabled]           DEFAULT (1),
        [EmailAddress]          VARCHAR(255)  NOT NULL,
        [OfflineTimeStart]      CHAR(5)       NOT NULL CONSTRAINT [DF_Operator_OTStart]             DEFAULT ('00:00'),
        [OfflineTimeEnd]        CHAR(5)       NOT NULL CONSTRAINT [DF_Operator_OTEnd]               DEFAULT ('00:00'),
        [IncludeMessageBody]    BIT           NOT NULL CONSTRAINT [DF_Operator_IncludeBody]         DEFAULT (1),
        [QueuedNotify]          TINYINT       NOT NULL CONSTRAINT [DF_Operator_QueuedNotify]        DEFAULT (1),
        [SummaryNotify]         BIT           NOT NULL                                              DEFAULT (0),
        [SummaryNotifyOK]       BIT           NOT NULL                                              DEFAULT (0),
        [SummaryNotifyWarn]     BIT           NOT NULL                                              DEFAULT (1),
        [SummaryNotifyFail]     BIT           NOT NULL                                              DEFAULT (1),
        [SummaryNotifyTime]     CHAR(5)       NOT NULL                                              DEFAULT ('08:00'),
        [SummaryNextNotifyDT]   DATETIME      NOT NULL                                              DEFAULT (GETDATE()),
        CONSTRAINT [PK_Operator] PRIMARY KEY CLUSTERED ([OperatorID] ASC)
    );
END
GO

-- ============================================================
-- MonitorOperator  (many-to-many)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MonitorOperator')
BEGIN
    CREATE TABLE [dbo].[MonitorOperator] (
        [MonitorID]   INT NOT NULL,
        [OperatorID]  INT NOT NULL,
        CONSTRAINT [PK_MonitorOperator] PRIMARY KEY CLUSTERED ([MonitorID] ASC, [OperatorID] ASC),
        CONSTRAINT [FK_MonitorOperator_Monitor]  FOREIGN KEY ([MonitorID])  REFERENCES [dbo].[Monitor]  ([MonitorID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonitorOperator_Operator] FOREIGN KEY ([OperatorID]) REFERENCES [dbo].[Operator] ([OperatorID]) ON DELETE CASCADE
    );
END
GO

-- ============================================================
-- SysSettings  (single-row settings table, key=Name)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SysSettings')
BEGIN
    CREATE TABLE [dbo].[SysSettings] (
        [Name]                      NVARCHAR(50)  NOT NULL,
        [ServiceServer]             NVARCHAR(255) NULL,
        [MainTimerInterval]         INT           NOT NULL CONSTRAINT [DF_SysSettings_Timer]       DEFAULT (60000),
        -- SMTP
        [SMTPFromName]              NVARCHAR(50)  NULL,
        [SMTPFromAddress]           NVARCHAR(255) NULL,
        [UseInternalSMTP]           BIT           NOT NULL                                         DEFAULT (1),
        [ExtSMTPServer]             NVARCHAR(255) NULL,
        [ExtSMTPPort]               INT           NULL,
        [ExtSMTPUserID]             NVARCHAR(50)  NULL,
        [ExtSMTPPwd]                NVARCHAR(50)  NULL,
        [ExtSMTPUseSSL]             BIT           NOT NULL                                         DEFAULT (0),
        -- Retention defaults (overridden per-monitor)
        [RetentionMaxMonthsRaw]     SMALLINT      NOT NULL CONSTRAINT [DF_SysSettings_RetRaw]      DEFAULT (24),
        [RetentionMaxMonthsDaily]   SMALLINT      NOT NULL CONSTRAINT [DF_SysSettings_RetDaily]    DEFAULT (36),
        [RetentionMaxMonthsWeekly]  SMALLINT      NOT NULL CONSTRAINT [DF_SysSettings_RetWeekly]   DEFAULT (60),
        [RetentionMaxMonthsMonthly] SMALLINT      NOT NULL CONSTRAINT [DF_SysSettings_RetMonthly]  DEFAULT (60),
        CONSTRAINT [PK_SysSettings] PRIMARY KEY CLUSTERED ([Name] ASC)
    );

    -- Seed default settings row
    INSERT INTO [dbo].[SysSettings] ([Name], [MainTimerInterval], [UseInternalSMTP])
    VALUES ('Default', 60000, 1);
END
GO

-- ============================================================
-- DashboardGroup
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DashboardGroup')
BEGIN
    CREATE TABLE [dbo].[DashboardGroup] (
        [GroupID]       INT           IDENTITY(1,1) NOT NULL,
        [Name]          NVARCHAR(100) NOT NULL,
        [DisplayOrder]  INT           NOT NULL CONSTRAINT [DF_DashboardGroup_Order] DEFAULT (0),
        CONSTRAINT [PK_DashboardGroup] PRIMARY KEY CLUSTERED ([GroupID] ASC)
    );
END
GO

-- ============================================================
-- DashboardGroupMonitor  (many-to-many)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DashboardGroupMonitor')
BEGIN
    CREATE TABLE [dbo].[DashboardGroupMonitor] (
        [GroupID]       INT NOT NULL,
        [MonitorID]     INT NOT NULL,
        [DisplayOrder]  INT NOT NULL CONSTRAINT [DF_DashboardGroupMonitor_Order] DEFAULT (0),
        CONSTRAINT [PK_DashboardGroupMonitor] PRIMARY KEY CLUSTERED ([GroupID] ASC, [MonitorID] ASC),
        CONSTRAINT [FK_DGM_Group]   FOREIGN KEY ([GroupID])   REFERENCES [dbo].[DashboardGroup] ([GroupID]) ON DELETE CASCADE,
        CONSTRAINT [FK_DGM_Monitor] FOREIGN KEY ([MonitorID]) REFERENCES [dbo].[Monitor]         ([MonitorID]) ON DELETE CASCADE
    );
END
GO

-- ============================================================
-- PropertyBag  (key-value store for application state)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PropertyBag')
BEGIN
    CREATE TABLE [dbo].[PropertyBag] (
        [PropertyKey]    NVARCHAR(255)  NOT NULL,
        [PropertyValue1] NVARCHAR(3000) NULL,
        [PropertyValue2] NTEXT          NULL,
        CONSTRAINT [PK_PropertyBag] PRIMARY KEY CLUSTERED ([PropertyKey] ASC)
    );
END
GO

PRINT 'PolyMon schema initialization complete.';
GO
