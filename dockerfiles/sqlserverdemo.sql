IF exists(select * from master..sysdatabases where [name]='s2dotnetdemo')
DROP DATABASE [s2dotnetdemo]
GO

CREATE DATABASE [s2dotnetdemo]
GO

use [s2dotnetdemo]
GO

CREATE TABLE [dbo].[DEPT] (
	[DEPTNO] numeric (2, 0) NOT NULL ,
	[DNAME] varchar (14) NULL ,
	[LOC] varchar (13) NULL ,
	[VERSIONNO] numeric (8, 0) NULL ,
	[ACTIVE] numeric (1, 0) NULL ,
	CONSTRAINT [PK_DEPT] PRIMARY KEY CLUSTERED ([DEPTNO]))
GO

CREATE TABLE [dbo].[DEPT2] (
	[DEPTNO] numeric (2, 0) NOT NULL ,
	[DNAME] varchar (14) NULL ,
	[ACTIVE] numeric (1, 0) NULL ,
	CONSTRAINT [PK_DEPT2] PRIMARY KEY CLUSTERED ([DEPTNO]))
GO

CREATE TABLE [dbo].[EMP] (
	[EMPNO] numeric (4, 0) NOT NULL ,
	[ENAME] varchar (10) NULL ,
	[JOB] varchar (9) NULL ,
	[MGR] numeric (4, 0) NULL ,
	[HIREDATE] datetime NULL ,
	[SAL] decimal (7, 2) NULL ,
	[COMM] numeric (7, 2) NULL ,
	[DEPTNO] numeric (2, 0) NULL ,
	[TSTAMP] datetime NULL ,
	CONSTRAINT [PK_EMP] PRIMARY KEY CLUSTERED ([EMPNO]))
GO

CREATE TABLE [dbo].[EMP2] (
	[EMPNO] numeric (4, 0) NOT NULL ,
	[ENAME] varchar (10) NULL ,
	[DEPTNUM] numeric (2, 0) NULL ,
	CONSTRAINT [PK_EMP2] PRIMARY KEY CLUSTERED ([EMPNO]))
GO

CREATE TABLE [dbo].[EMP_NULLABLE] (
	[EMPNO] numeric (4, 0) NOT NULL ,
	[ENAME] varchar (10) NULL ,
	[JOB] varchar (9) NULL ,
	[MGR] numeric (4, 0) NULL ,
	[HIREDATE] datetime NULL ,
	[SAL] decimal (7, 2) NULL ,
	[COMM] numeric (7, 2) NULL ,
	[DEPTNO] numeric (2, 0) NULL ,
	[TSTAMP] datetime NOT NULL ,
	[NULLABLENEXTRESTDATE] datetime NULL,
	CONSTRAINT [PK_EMP_NULLABLE] PRIMARY KEY CLUSTERED ([EMPNO]))
GO

CREATE TABLE [dbo].[GENERIC_NULLABLE](
	[ID] numeric (38, 0) IDENTITY (1, 1) NOT NULL,
	[DDATE] datetime NULL,
	[ENTITYNO] int NULL,
	CONSTRAINT [PK_GENERIC_NULLABLE] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) ON [PRIMARY],
	CONSTRAINT [AK_GENERRIC_NULLABLE_ENTITY_NO] UNIQUE NONCLUSTERED 
	(
		[ENTITYNO] ASC
	) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[UNDER_SCORE](
	[UNDER_SCORE_NO] numeric (4, 0) NOT NULL,
	[TABLE_NAME] varchar(50) NULL,
	[TABLE_NAME_] varchar(50) NULL,
	[_TABLE_NAME] varchar(50) NULL,
	[_TABLE_NAME_] varchar(50) NULL,
	CONSTRAINT [PK_UNDER_SCORE] PRIMARY KEY CLUSTERED 
	(
		[UNDER_SCORE_NO] ASC
	) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[IDTABLE] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[ID_NAME] varchar (20) NULL ,
	CONSTRAINT [PK_IDTABLE] PRIMARY KEY CLUSTERED ([ID]))
GO

CREATE TABLE [dbo].[BASICTYPE] (
	[ID] numeric (18, 0) NOT NULL,
	[BOOLTYPE] bit NULL,
	[BYTETYPE] tinyint NULL,
	[SBYTETYPE] numeric (3, 0) NULL,
	[INT16TYPE] smallint NULL,
	[INT32TYPE] int NULL,
	[INT64TYPE] bigint NULL,
	[SINGLETYPE] float NULL,
	[DOUBLETYPE] DOUBLE PRECISION NULL,
	[DECIMALTYPE] decimal (28, 0) NULL,
	[STRINGTYPE] varchar (1024) NULL,
	[DATETIMETYPE] datetime NULL,
	CONSTRAINT [PK_BASICTYPE] PRIMARY KEY NONCLUSTERED ([ID])
)
GO

CREATE TABLE [dbo].[DECIMAL_VERSION_NO] (
	[EMPNO] numeric (4, 0) NOT NULL ,
	[EMP_NAME] varchar(60) NULL,
	[VERSION_NO] numeric (4, 0) NOT NULL,
	CONSTRAINT [PK_DECIMAL_VERSION_NO] PRIMARY KEY NONCLUSTERED ([EMPNO])
)
GO

CREATE TABLE [dbo].[INT_VERSION_NO] (
	[EMPNO] numeric (4, 0) NOT NULL ,
	[EMP_NAME] varchar(60) NULL,
	[VERSION_NO] int NOT NULL,
	CONSTRAINT [PK_INT_VERSION_NO] PRIMARY KEY NONCLUSTERED ([EMPNO])
)
GO

CREATE TABLE [dbo].[EMP_DEFAULT] (
	[EMPNO] numeric (4, 0) NOT NULL,
	[ENAME] varchar (10) NOT NULL CONSTRAINT [DF_EMP_DEFAULT_ENAME]  DEFAULT ('def_name'),
	[JOB] varchar (9) NULL,
	[MGR] numeric (4, 0) NULL CONSTRAINT [DF_EMP_DEFAULT_MGR]  DEFAULT ((99)),
	[SAL] decimal (7, 2) NOT NULL CONSTRAINT [DF_EMP_DEFAULT_SAL]  DEFAULT ((9.99)),
	[COMM] numeric (7, 2) NULL,
	[DEPTNO] numeric (2, 0) NULL,
	[TSTAMP] datetime NULL,
	[VERSION] numeric (4, 0) NULL,
	CONSTRAINT [PK_EMP_DEFAULT] PRIMARY KEY NONCLUSTERED ([EMPNO])
)
GO

INSERT INTO [dbo].[EMP] VALUES(7369,'SMITH','CLERK',7902,CONVERT(datetime,'1980-12-17'),800,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7499,'ALLEN','SALESMAN',7698,CONVERT(datetime,'1981-02-20'),1600,300,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7521,'WARD','SALESMAN',7698,CONVERT(datetime,'1981-02-22'),1250,500,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7566,'JONES','MANAGER',7839,CONVERT(datetime,'1981-04-02'),2975,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7654,'MARTIN','SALESMAN',7698,CONVERT(datetime,'1981-09-28'),1250,1400,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7698,'BLAKE','MANAGER',7839,CONVERT(datetime,'1981-05-01'),2850,NULL,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7782,'CLARK','MANAGER',7839,CONVERT(datetime,'1981-06-09'),2450,NULL,10,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7788,'SCOTT','ANALYST',7566,CONVERT(datetime,'1982-12-09'),3000.0,NULL,20,CONVERT(datetime,'2005-01-18 13:09:32.213'))
INSERT INTO [dbo].[EMP] VALUES(7839,'KING','PRESIDENT',NULL,CONVERT(datetime,'1981-11-17'),5000,NULL,10,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7844,'TURNER','SALESMAN',7698,CONVERT(datetime,'1981-09-08'),1500,0,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7876,'ADAMS','CLERK',7788,CONVERT(datetime,'1983-01-12'),1100,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7900,'JAMES','CLERK',7698,CONVERT(datetime,'1981-12-03'),950,NULL,30,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7902,'FORD','ANALYST',7566,CONVERT(datetime,'1981-12-03'),3000,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[EMP] VALUES(7934,'MILLER','CLERK',7782,CONVERT(datetime,'1982-01-23'),1300,NULL,10,CONVERT(datetime,'2000-01-01 00:00:00.0'))
INSERT INTO [dbo].[DEPT] VALUES(10,'ACCOUNTING','NEW YORK',0,1)
INSERT INTO [dbo].[DEPT] VALUES(20,'RESEARCH','DALLAS',0,1)
INSERT INTO [dbo].[DEPT] VALUES(30,'SALES','CHICAGO',0,1)
INSERT INTO [dbo].[DEPT] VALUES(40,'OPERATIONS','BOSTON',0,1)
INSERT INTO [dbo].[EMP2] VALUES(7369,'SMITH',20)
INSERT INTO [dbo].[EMP2] VALUES(7499,'ALLEN',30)
INSERT INTO [dbo].[EMP_NULLABLE] VALUES(1,'ADAMS','CLERK',7788,CONVERT(datetime,'1983-01-12'),1100,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'),CONVERT(datetime,'2006-11-07'))
INSERT INTO [dbo].[EMP_NULLABLE] VALUES(10,'JAMES','CLERK',7698,CONVERT(datetime,'1981-12-03'),950,NULL,30,CONVERT(datetime,'2000-01-01 00:00:00.0'),null)
INSERT INTO [dbo].[EMP_NULLABLE] VALUES(100,'FORD','ANALYST',7566,CONVERT(datetime,'1981-12-03'),3000,NULL,20,CONVERT(datetime,'2000-01-01 00:00:00.0'),CONVERT(datetime,'2006-11-07'))
INSERT INTO [dbo].[GENERIC_NULLABLE] (DDATE, ENTITYNO) VALUES(getdate(),100)
INSERT INTO [dbo].[GENERIC_NULLABLE] (DDATE, ENTITYNO) VALUES(NULL,101)
INSERT INTO [dbo].[UNDER_SCORE] VALUES(1,'table_name','table_name_','_table_name','_table_name_')
INSERT INTO [dbo].[DEPT2] VALUES(20,'RESEARCH',1)
INSERT INTO [dbo].[DEPT2] VALUES(30,'SALES',0)
INSERT INTO [dbo].[BASICTYPE] VALUES (
	1,
	0,
	255,
	-128,
	32767,
	2147483647,
	9223372036854775807,
	9.876543,
	9.87654321098765,
	9999999999999999999999999999,
	'竹\薮～',
	CONVERT(datetime, '1980-12-17 12:34:56')
);
INSERT INTO [dbo].[DECIMAL_VERSION_NO] VALUES(10, 'Decimal', 100);
INSERT INTO [dbo].[INT_VERSION_NO] VALUES(10, 'Int', 100);

IF OBJECT_ID ( 'dbo.sales_tax2' ) IS NOT NULL
DROP FUNCTION dbo.sales_tax2
GO

CREATE FUNCTION dbo.sales_tax2 (@sales real)
RETURNS real
AS
BEGIN
RETURN @sales * 0.2;
END
GO

IF OBJECT_ID ( 'dbo.sales_tax' ) IS NOT NULL
DROP PROCEDURE dbo.sales_tax
GO

CREATE PROCEDURE dbo.sales_tax (@sales real, @tax real OUTPUT)
AS
BEGIN
SET @tax = @sales * 0.2;
END
GO

IF OBJECT_ID ( 'dbo.sales_tax3' ) IS NOT NULL
DROP PROCEDURE dbo.sales_tax3
GO

CREATE PROCEDURE dbo.sales_tax3 (@sales real OUTPUT)
AS
BEGIN
SET @sales = @sales * 0.2;
END
GO

IF OBJECT_ID ( 'dbo.sales_tax4' ) IS NOT NULL
DROP PROCEDURE dbo.sales_tax4
GO

CREATE PROCEDURE dbo.sales_tax4 (@sales real, @tax real OUTPUT, @total real OUTPUT)
AS
BEGIN
SET @tax = @sales * 0.2;
SET @total = @sales * 1.2;
END
GO

/****** Object:  StoredProcedure [dbo].[SelectForOutputParam]    Script Date: 03/23/2013 14:45:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<koyak>
-- Create date: <2013/03/23>
-- Description:	<outパラメータ設定テスト>
-- =============================================
CREATE PROCEDURE [dbo].[SelectForOutputParam]
	-- Add the parameters for the stored procedure here
	@Mgr numeric(4,0)OUTPUT,
	@Empno numeric(4,0)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @Mgr=MGR
	FROM dbo.EMP
	WHERE EMPNO=@Empno
END
GO

/****** Object:  StoredProcedure [dbo].[SelectForOutputParam]    Script Date: 03/30/2013 13:34:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<koyak>
-- Create date: <2013/03/23>
-- Description:	<outパラメータ設定テスト>
-- =============================================
ALTER PROCEDURE [dbo].[SelectForOutputParam]
	-- Add the parameters for the stored procedure here
	@Mgr numeric(4,0)OUTPUT,
	@Empno numeric(4,0)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT @Mgr=MGR
	FROM dbo.EMP
	WHERE EMPNO=@Empno
END

GO
/****** Object:  StoredProcedure [dbo].[SelectForOutputParamMulti]    Script Date: 03/30/2013 13:34:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<koyak>
-- Create date: <2013/03/23>
-- Description:	<outパラメータ取得テスト（複数行取得）>
-- =============================================
CREATE PROCEDURE [dbo].[SelectForOutputParamMulti]
	-- Add the parameters for the stored procedure here
	@Mgr numeric(4,0) OUTPUT,
	@Job varchar(9),
	@TestValue numeric(4,0)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET @Mgr=@TestValue;
    -- Insert statements for procedure here
	SELECT MGR
	FROM dbo.EMP
	WHERE JOB=@Job;
END
