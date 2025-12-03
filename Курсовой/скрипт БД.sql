USE [master]
GO
/****** Object:  Database [CourseProject]    Script Date: 03.12.2025 7:59:28 ******/
CREATE DATABASE [CourseProject]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'CourseProject', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\CourseProject.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'CourseProject_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\CourseProject_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [CourseProject] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CourseProject].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CourseProject] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CourseProject] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CourseProject] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CourseProject] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CourseProject] SET ARITHABORT OFF 
GO
ALTER DATABASE [CourseProject] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [CourseProject] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CourseProject] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CourseProject] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CourseProject] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CourseProject] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CourseProject] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CourseProject] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CourseProject] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CourseProject] SET  DISABLE_BROKER 
GO
ALTER DATABASE [CourseProject] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CourseProject] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CourseProject] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CourseProject] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CourseProject] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CourseProject] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [CourseProject] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CourseProject] SET RECOVERY FULL 
GO
ALTER DATABASE [CourseProject] SET  MULTI_USER 
GO
ALTER DATABASE [CourseProject] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CourseProject] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CourseProject] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CourseProject] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CourseProject] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CourseProject] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'CourseProject', N'ON'
GO
ALTER DATABASE [CourseProject] SET QUERY_STORE = ON
GO
ALTER DATABASE [CourseProject] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [CourseProject]
GO
/****** Object:  Table [dbo].[Request]    Script Date: 03.12.2025 7:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Request](
	[IdRequest] [int] IDENTITY(1,1) NOT NULL,
	[RequestDate] [datetime] NOT NULL,
	[Address] [nvarchar](100) NOT NULL,
	[TelephoneNumber] [nvarchar](20) NOT NULL,
	[IdUser] [int] NULL,
	[CountersNumber] [int] NOT NULL,
	[Comment] [nvarchar](100) NULL,
	[Master] [nvarchar](50) NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Request] PRIMARY KEY CLUSTERED 
(
	[IdRequest] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 03.12.2025 7:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[IdUser] [int] IDENTITY(1,1) NOT NULL,
	[TelephoneNumber] [nvarchar](20) NOT NULL,
	[UserLogin] [nvarchar](50) NOT NULL,
	[UserPassword] [nvarchar](50) NOT NULL,
	[UserSurname] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[UserPatronymic] [nvarchar](50) NULL,
	[IdRole] [int] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[IdUser] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_RequestsWithUserPhones]    Script Date: 03.12.2025 7:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_RequestsWithUserPhones] AS SELECT r.IdRequest, r.RequestDate, r.Address, u.TelephoneNumber AS UserPhone, r.CountersNumber, r.Comment, r.Master, r.Status FROM Request r INNER JOIN [User] u ON r.IdUser = u.IdUser;
GO
/****** Object:  View [dbo].[RequestsWithUserPhones]    Script Date: 03.12.2025 7:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[RequestsWithUserPhones] AS
SELECT 
    r.IdRequest,
    r.RequestDate,
    r.Address,
    u.TelephoneNumber AS UserPhone, 
    r.CountersNumber,
    r.Comment,
    r.Master,
    r.Status
FROM Request r
INNER JOIN [User] u ON r.IdUser = u.IdUser;
GO
/****** Object:  Table [dbo].[Role]    Script Date: 03.12.2025 7:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[IdRole] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[IdRole] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Request] ON 
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (5, CAST(N'2025-11-19T00:00:00.000' AS DateTime), N'Воскресенская 75к1 кв.21', N'8952341273', 1, 2, N'-', N'Логинов Иван Сергеевич', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (6, CAST(N'2025-11-22T00:00:00.000' AS DateTime), N'Обводный канал д.44 кв.22', N'4', 3, 4, N'-', N'вапвап вапвапвап апвпвап', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (8, CAST(N'2025-11-11T00:00:00.000' AS DateTime), N'попова д.25 кв.43', N'4', 3, 1, N'-', N'Логинов Иван Сергеевич', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (9, CAST(N'2025-11-28T01:33:13.343' AS DateTime), N'Воскресенская д.59 кв.354', N'54363634643', 8, 3, NULL, N'вапвап вапвапвап апвпвап', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (10, CAST(N'2025-11-28T03:27:40.233' AS DateTime), N'Логинова д.23 кв.55', N'54363634643', 8, 4, NULL, N'Логинов Иван Сергеевич', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (13, CAST(N'2025-12-01T15:14:05.840' AS DateTime), N'fsdfsd', N'54363634643', 8, 3, N'написать СМС', N'вапвап вапвапвап апвпвап', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (14, CAST(N'2025-12-02T00:47:49.797' AS DateTime), N'Логинова д.23 кв.56', N'89523965826', 10, 2, N'ааааааа', NULL, N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (15, CAST(N'2025-12-02T00:48:14.573' AS DateTime), N'Попова д.25 кв.67', N'89523965826', 10, 2, NULL, N'Логинов Иван Сергеевич', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (16, CAST(N'2025-12-02T00:50:48.900' AS DateTime), N'Воскресенская д.6 кв.16', N'89523965826', 10, 3, NULL, N'вапвап вапвапвап апвпвап', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (17, CAST(N'2025-12-02T01:00:04.763' AS DateTime), N'Суфтина д.33 кв.25', N'889315621344', 1, 2, NULL, N'вапвап вапвапвап апвпвап', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (18, CAST(N'2025-12-02T01:00:58.070' AS DateTime), N'Ломоносова д.98 кв.324', N'89115563784', 10, 4, NULL, N'вапвап вапвапвап апвпвап', N'В работе')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (19, CAST(N'2025-12-02T01:08:32.670' AS DateTime), N'Поморская д.34к2 кв.5', N'89006703327', 8, 2, NULL, N'Логинов Иван Сергеевич', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (20, CAST(N'2025-12-02T01:08:57.657' AS DateTime), N'Воскресенская д.59 кв.390', N'89115576823', 8, 2, NULL, N'Логинов Иван Сергеевич', N'Выполнена')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (23, CAST(N'2025-12-03T01:25:29.083' AS DateTime), N'Панина д.32к2 кв.35', N'89561235343', 1, 100, NULL, NULL, N'Новая')
GO
INSERT [dbo].[Request] ([IdRequest], [RequestDate], [Address], [TelephoneNumber], [IdUser], [CountersNumber], [Comment], [Master], [Status]) VALUES (24, CAST(N'2025-12-03T01:27:44.597' AS DateTime), N'ffsdgdfgdf', N'86541233232', 2, -1, NULL, NULL, N'Новая')
GO
SET IDENTITY_INSERT [dbo].[Request] OFF
GO
SET IDENTITY_INSERT [dbo].[Role] ON 
GO
INSERT [dbo].[Role] ([IdRole], [RoleName]) VALUES (1, N'Администратор')
GO
INSERT [dbo].[Role] ([IdRole], [RoleName]) VALUES (2, N'Менеджер')
GO
INSERT [dbo].[Role] ([IdRole], [RoleName]) VALUES (3, N'Мастер-поверитель')
GO
INSERT [dbo].[Role] ([IdRole], [RoleName]) VALUES (4, N'Зарегистрированный пользователь')
GO
SET IDENTITY_INSERT [dbo].[Role] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (1, N'8952341273', N'admin', N'admin', N'Кигерс', N'Нилл', N'куклускланович', 1)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (2, N'3', N'manager', N'123', N'Кыхтышпв', N'агкртовы', N'уэээээ', 2)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (3, N'4', N'master123', N'qwerty', N'вапвап', N'вапвапвап', N'апвпвап', 3)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (4, N'52352323', N'User1', N'123', N'Абдщхаб', N'Абдул', N'Зубханович', 4)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (8, N'54363634643', N'Yarab', N'123', N'Иванов', N'Иван', NULL, 4)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (9, N'89526373285', N'master2', N'123', N'Логинов', N'Иван', N'Сергеевич', 3)
GO
INSERT [dbo].[User] ([IdUser], [TelephoneNumber], [UserLogin], [UserPassword], [UserSurname], [UserName], [UserPatronymic], [IdRole]) VALUES (10, N'89523965826', N'user2', N'123', N'Петров', N'Владислав', N'Михайлович', 4)
GO
SET IDENTITY_INSERT [dbo].[User] OFF
GO
ALTER TABLE [dbo].[Request]  WITH CHECK ADD  CONSTRAINT [FK_Request_User1] FOREIGN KEY([IdUser])
REFERENCES [dbo].[User] ([IdUser])
GO
ALTER TABLE [dbo].[Request] CHECK CONSTRAINT [FK_Request_User1]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Role] FOREIGN KEY([IdRole])
REFERENCES [dbo].[Role] ([IdRole])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Role]
GO
USE [master]
GO
ALTER DATABASE [CourseProject] SET  READ_WRITE 
GO
