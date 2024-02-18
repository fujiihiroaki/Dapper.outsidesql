CREATE TABLE DEPT (
    DEPTNO smallint NOT NULL,
    DNAME varchar(14),
    LOC varchar(14),
    VERSIONNO integer,
    ACTIVE boolean,
    CONSTRAINT PK_DEPT PRIMARY KEY (DEPTNO)
);

CREATE TABLE DEPT2 (
    DEPTNO smallint NOT NULL,
    DNAME varchar(14),
    ACTIVE boolean,
    CONSTRAINT PK_DEPT2 PRIMARY KEY (DEPTNO)
);

CREATE TABLE EMP (
    EMPNO integer NOT NULL,
    ENAME varchar(10),
    JOB varchar(10),
    MGR smallint,
    HIREDATE timestamp,
    SAL numeric(7, 2),
    COMM numeric(7, 2),
    DEPTNO smallint,
    TSTAMP timestamp,
    CONSTRAINT PK_EMP PRIMARY KEY (EMPNO)
);

CREATE TABLE EMP2 (
    EMPNO integer NOT NULL,
    ENAME varchar(10),
    DEPTNUM smallint,
    CONSTRAINT PK_EMP2 PRIMARY KEY (EMPNO)
);

CREATE TABLE EMP_NULLABLE (
    EMPNO integer NOT NULL,
    ENAME varchar(10),
    JOB varchar(10),
    MGR smallint,
    HIREDATE timestamp,
    SAL numeric(7, 2),
    COMM numeric(7, 2),
    DEPTNO smallint,
    TSTAMP timestamp NOT NULL,
    NULLABLENEXTRESTDATE timestamp,
    CONSTRAINT PK_EMP_NULLABLE PRIMARY KEY (EMPNO)
);

CREATE TABLE GENERIC_NULLABLE (
    ID numeric(31, 0) NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
    DDATE timestamp,
    ENTITYNO integer,
    CONSTRAINT PK_GENERIC_NULLABLE PRIMARY KEY (ID)
);

CREATE TABLE UNDER_SCORE (
    UNDER_SCORE_NO smallint NOT NULL,
    TABLE_NAME varchar(50),
    "TABLE_NAME_" varchar(50),
    "_TABLE_NAME" varchar(50),
    "_TABLE_NAME_" varchar(50),
    CONSTRAINT PK_UNDER_SCORE PRIMARY KEY (UNDER_SCORE_NO)
);

CREATE TABLE IDTABLE (
    ID integer NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
    DDATE timestamp,
    ENTITYNO integer,
    CONSTRAINT PK_IDTABLE PRIMARY KEY (ID)
);

CREATE TABLE BASICTYPE (
    ID numeric(18, 0) NOT NULL,
    BOOLTYPE boolean,
    BYTETYPE smallint,
    SBYTETYPE integer,
    INT16TYPE integer,
    INT32TYPE integer,
    INT64TYPE numeric(19, 0),
    SINGLETYPE float(8),
    DOUBLETYPE DOUBLE PRECISION,
    DECIMALTYPE numeric(28, 0),
    STRINGTYPE varchar(1024),
    DATETIMETYPE timestamp,
    CONSTRAINT PK_BASICTYPE PRIMARY KEY (ID)
);

INSERT INTO EMP VALUES (7369, 'SMITH',  'CLERK',       7902, '1980-12-17 00:00:00',  800, NULL, 20, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7499, 'ALLEN',  'SALESMAN',    7698, '1981-02-20 00:00:00', 1600,  300, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7521, 'WARD',   'SALESMAN',    7698, '1981-02-22 00:00:00', 1250,  500, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7566, 'JONES',  'MANAGER',     7839, '1981-04-02 00:00:00', 2975, NULL, 20, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7654, 'MARTIN', 'SALESMAN',    7698, '1981-09-28 00:00:00', 1250, 1400, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7698, 'BLAKE',  'MANAGER',     7839, '1981-05-01 00:00:00', 2850, NULL, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7782, 'CLARK',  'MANAGER',     7839, '1981-06-09 00:00:00', 2450, NULL, 10, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7788, 'SCOTT',  'ANALYST',     7566, '1982-12-09 00:00:00', 3000, NULL, 20, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7839, 'KING',   'PRESIDENT',   NULL, '1981-11-17 00:00:00', 5000, NULL, 10, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7844, 'TURNER', 'SALESMAN',    7698, '1981-09-08 00:00:00', 1500,    0, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7876, 'ADAMS',  'CLERK',       7788, '1983-01-12 00:00:00', 1100, NULL, 20, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7900, 'JAMES',  'CLERK',       7698, '1981-12-03 00:00:00',  950, NULL, 30, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7902, 'FORD',   'ANALYST',     7566, '1981-12-03 00:00:00', 3000, NULL, 20, '2000-01-01 00:00:00');
INSERT INTO EMP VALUES (7934, 'MILLER', 'CLERK',       7782, '1982-01-23 00:00:00', 1300, NULL, 10, '2000-01-01 00:00:00');
INSERT INTO DEPT VALUES (10, 'ACCOUNTING', 'NEW YORK', 0, true);
INSERT INTO DEPT VALUES (20, 'RESEARCH',   'DALLAS',   0, true);
INSERT INTO DEPT VALUES (30, 'SALES',      'CHICAGO',  0, true);
INSERT INTO DEPT VALUES (40, 'OPERATIONS', 'BOSTON',   0, true);
INSERT INTO EMP2 VALUES (7369, 'SMITH', 20);
INSERT INTO EMP2 VALUES (7499, 'ALLEN', 30);
INSERT INTO EMP_NULLABLE VALUES (1, 'ADAMS', 'CLERK', 7788, '1983-01-12 00:00:00', 1100, NULL, 20, '2000-01-01 00:00:00', '2006-11-07 00:00:00');
INSERT INTO EMP_NULLABLE VALUES (10, 'JAMES', 'CLERK', 7698, '1981-12-03 00:00:00', 950, NULL, 30, '2000-01-01 00:00:00', null);
INSERT INTO EMP_NULLABLE VALUES (100, 'FORD', 'ANALYST', 7566, '1981-12-03 00:00:00', 3000, NULL, 20, '2000-01-01 00:00:00', '2006-11-07 00:00:00');
INSERT INTO GENERIC_NULLABLE (DDATE, ENTITYNO) VALUES ('2000-12-31 12:34:56', 100);
INSERT INTO GENERIC_NULLABLE (DDATE, ENTITYNO) VALUES (null, 101);
INSERT INTO UNDER_SCORE VALUES (1,'table_name','table_name_','_table_name','_table_name_');
INSERT INTO DEPT2 VALUES (20, 'RESEARCH', true);
INSERT INTO DEPT2 VALUES (30, 'SALES',    false);
INSERT INTO BASICTYPE VALUES (
    1,
    false,
    255,
    -128,
    32767,
    2147483647,
    9223372036854775807,
    9.876543,
    9.87654321098765,
    9999999999999999999999999999,
    '竹薮～',
    '1980-12-17 12:34:56'
);

INSERT INTO BASICTYPE (
    id
) VALUES (
    2
);

INSERT INTO BASICTYPE VALUES (
    3,
    true,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9,
    '10',
    NULL
);

