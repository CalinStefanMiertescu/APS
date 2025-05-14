CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [DateOfBirth] datetime2 NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [JournalistType] nvarchar(50) NOT NULL,
    [OriginalJournalistType] nvarchar(max) NULL,
    [Publication] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [IsEmailVerified] bit NOT NULL,
    [EmailVerificationToken] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsAdmin] bit NOT NULL,
    [IsPayingMember] bit NOT NULL,
    [MembershipExpiresAt] datetime2 NULL,
    [LastMembershipPayment] datetime2 NULL,
    [HasPendingChanges] bit NOT NULL,
    [PendingChangesJson] nvarchar(max) NULL,
    [IsRejected] bit NOT NULL,
    [RejectedAt] datetime2 NULL,
    [RejectionReason] nvarchar(max) NULL,
    [IsModerator] bit NOT NULL,
    [ProfilePictureUrl] nvarchar(max) NULL,
    [Biography] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Announcements] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedById] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Announcements_AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Audit_Logs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Action] nvarchar(255) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Audit_Logs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Audit_Logs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [IsPaid] bit NOT NULL,
    [ExpirationDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Articles] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [AuthorId] nvarchar(450) NOT NULL,
    [CategoryId] int NOT NULL,
    [CoverImageUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [PublishedAt] datetime2 NULL,
    [IsPublished] bit NOT NULL,
    [IsHighlighted] bit NOT NULL,
    [ViewCount] int NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Articles_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Articles_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ArticleComment] (
    [Id] int NOT NULL IDENTITY,
    [ArticleId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsApproved] bit NOT NULL,
    CONSTRAINT [PK_ArticleComment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleComment_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArticleComment_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ArticleImage] (
    [Id] int NOT NULL IDENTITY,
    [ArticleId] int NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [Caption] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    CONSTRAINT [PK_ArticleImage] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ArticleImage_Articles_ArticleId] FOREIGN KEY ([ArticleId]) REFERENCES [Articles] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_Announcements_CreatedById] ON [Announcements] ([CreatedById]);
GO


CREATE INDEX [IX_ArticleComment_ArticleId] ON [ArticleComment] ([ArticleId]);
GO


CREATE INDEX [IX_ArticleComment_UserId] ON [ArticleComment] ([UserId]);
GO


CREATE INDEX [IX_ArticleImage_ArticleId] ON [ArticleImage] ([ArticleId]);
GO


CREATE INDEX [IX_Articles_AuthorId] ON [Articles] ([AuthorId]);
GO


CREATE INDEX [IX_Articles_CategoryId] ON [Articles] ([CategoryId]);
GO


CREATE INDEX [IX_Articles_Title] ON [Articles] ([Title]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Audit_Logs_UserId] ON [Audit_Logs] ([UserId]);
GO


CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
GO


CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
GO


