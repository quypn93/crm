-- =====================================================
-- CRM Database Schema
-- Microsoft SQL Server
-- =====================================================

-- Create Database (run separately if needed)
-- CREATE DATABASE CRM_Dev;
-- GO
-- USE CRM_Dev;
-- GO

-- =====================================================
-- Users Table
-- =====================================================
CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [AvatarUrl] NVARCHAR(500) NULL,
    [IsActive] BIT DEFAULT 1,
    [RefreshToken] NVARCHAR(500) NULL,
    [RefreshTokenExpiryTime] DATETIME2 NULL,
    [LastLoginAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL
);

CREATE INDEX IX_Users_Email ON [dbo].[Users]([Email]);

-- =====================================================
-- Roles Table
-- =====================================================
CREATE TABLE [dbo].[Roles] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(50) NOT NULL UNIQUE,
    [Description] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL
);

-- Seed default roles
INSERT INTO [dbo].[Roles] ([Id], [Name], [Description]) VALUES
    (NEWID(), 'Admin', 'System Administrator with full access'),
    (NEWID(), 'SalesManager', 'Sales Manager with team oversight'),
    (NEWID(), 'SalesRep', 'Sales Representative with standard access');

-- =====================================================
-- UserRoles Junction Table
-- =====================================================
CREATE TABLE [dbo].[UserRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedAt] DATETIME2 DEFAULT GETUTCDATE(),
    PRIMARY KEY ([UserId], [RoleId]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
);

-- =====================================================
-- Customers Table
-- =====================================================
CREATE TABLE [dbo].[Customers] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Email] NVARCHAR(255) NULL,
    [Phone] NVARCHAR(50) NULL,
    [Address] NVARCHAR(500) NULL,
    [City] NVARCHAR(100) NULL,
    [Country] NVARCHAR(100) NULL,
    [PostalCode] NVARCHAR(20) NULL,
    [CompanyName] NVARCHAR(255) NULL,
    [Industry] NVARCHAR(100) NULL,
    [Website] NVARCHAR(255) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [CreatedByUserId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedToUserId] UNIQUEIDENTIFIER NULL,
    FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users]([Id]),
    FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([Id])
);

CREATE INDEX IX_Customers_Name ON [dbo].[Customers]([Name]);
CREATE INDEX IX_Customers_Email ON [dbo].[Customers]([Email]);
CREATE INDEX IX_Customers_CompanyName ON [dbo].[Customers]([CompanyName]);
CREATE INDEX IX_Customers_AssignedToUserId ON [dbo].[Customers]([AssignedToUserId]);

-- =====================================================
-- DealStages Table (Pipeline Stages)
-- =====================================================
CREATE TABLE [dbo].[DealStages] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Order] INT NOT NULL,
    [Color] NVARCHAR(20) NULL,
    [Probability] INT DEFAULT 0,
    [IsDefault] BIT DEFAULT 0,
    [IsWonStage] BIT DEFAULT 0,
    [IsLostStage] BIT DEFAULT 0,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL
);

-- Seed default deal stages
INSERT INTO [dbo].[DealStages] ([Id], [Name], [Order], [Color], [Probability], [IsDefault]) VALUES
    (NEWID(), 'Lead', 1, '#6366F1', 10, 1),
    (NEWID(), 'Qualified', 2, '#8B5CF6', 25, 0),
    (NEWID(), 'Proposal', 3, '#EC4899', 50, 0),
    (NEWID(), 'Negotiation', 4, '#F59E0B', 75, 0);

INSERT INTO [dbo].[DealStages] ([Id], [Name], [Order], [Color], [Probability], [IsWonStage]) VALUES
    (NEWID(), 'Won', 5, '#10B981', 100, 1);

INSERT INTO [dbo].[DealStages] ([Id], [Name], [Order], [Color], [Probability], [IsLostStage]) VALUES
    (NEWID(), 'Lost', 6, '#EF4444', 0, 1);

-- =====================================================
-- Deals Table
-- =====================================================
CREATE TABLE [dbo].[Deals] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Title] NVARCHAR(255) NOT NULL,
    [Value] DECIMAL(18, 2) NOT NULL DEFAULT 0,
    [Currency] NVARCHAR(10) DEFAULT 'VND',
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [StageId] UNIQUEIDENTIFIER NOT NULL,
    [ExpectedCloseDate] DATE NULL,
    [ActualCloseDate] DATE NULL,
    [Probability] INT DEFAULT 0,
    [Notes] NVARCHAR(MAX) NULL,
    [LostReason] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [CreatedByUserId] UNIQUEIDENTIFIER NOT NULL,
    [AssignedToUserId] UNIQUEIDENTIFIER NULL,
    FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers]([Id]),
    FOREIGN KEY ([StageId]) REFERENCES [dbo].[DealStages]([Id]),
    FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users]([Id]),
    FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([Id])
);

CREATE INDEX IX_Deals_CustomerId ON [dbo].[Deals]([CustomerId]);
CREATE INDEX IX_Deals_StageId ON [dbo].[Deals]([StageId]);
CREATE INDEX IX_Deals_AssignedToUserId ON [dbo].[Deals]([AssignedToUserId]);
CREATE INDEX IX_Deals_ExpectedCloseDate ON [dbo].[Deals]([ExpectedCloseDate]);

-- =====================================================
-- Tasks Table
-- =====================================================
CREATE TABLE [dbo].[Tasks] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [DueDate] DATETIME2 NULL,
    [ReminderDate] DATETIME2 NULL,
    [Priority] INT DEFAULT 1,  -- 1: Low, 2: Medium, 3: High, 4: Urgent
    [Status] INT DEFAULT 0,    -- 0: Pending, 1: InProgress, 2: Completed, 3: Cancelled
    [CustomerId] UNIQUEIDENTIFIER NULL,
    [DealId] UNIQUEIDENTIFIER NULL,
    [AssignedToUserId] UNIQUEIDENTIFIER NULL,
    [CreatedByUserId] UNIQUEIDENTIFIER NOT NULL,
    [CompletedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers]([Id]) ON DELETE SET NULL,
    FOREIGN KEY ([DealId]) REFERENCES [dbo].[Deals]([Id]) ON DELETE SET NULL,
    FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([Id]),
    FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users]([Id])
);

CREATE INDEX IX_Tasks_CustomerId ON [dbo].[Tasks]([CustomerId]);
CREATE INDEX IX_Tasks_DealId ON [dbo].[Tasks]([DealId]);
CREATE INDEX IX_Tasks_AssignedToUserId ON [dbo].[Tasks]([AssignedToUserId]);
CREATE INDEX IX_Tasks_DueDate ON [dbo].[Tasks]([DueDate]);
CREATE INDEX IX_Tasks_Status ON [dbo].[Tasks]([Status]);

-- =====================================================
-- ActivityLogs Table (Audit Trail)
-- =====================================================
CREATE TABLE [dbo].[ActivityLogs] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [EntityType] NVARCHAR(50) NOT NULL,
    [EntityId] UNIQUEIDENTIFIER NOT NULL,
    [Action] NVARCHAR(50) NOT NULL,
    [OldValue] NVARCHAR(MAX) NULL,
    [NewValue] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(500) NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);

CREATE INDEX IX_ActivityLogs_EntityType_EntityId ON [dbo].[ActivityLogs]([EntityType], [EntityId]);
CREATE INDEX IX_ActivityLogs_UserId ON [dbo].[ActivityLogs]([UserId]);
CREATE INDEX IX_ActivityLogs_CreatedAt ON [dbo].[ActivityLogs]([CreatedAt]);
