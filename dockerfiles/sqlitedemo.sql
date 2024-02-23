DROP TABLE IF EXISTS DEPT;
CREATE TABLE DEPT (
	DEPTNO NUMERIC NOT NULL,
	DNAME TEXT NULL,
	LOC TEXT NULL,
	VERSIONNO NUMERIC NULL,
	ACTIVE NUMERIC NULL,
	PRIMARY KEY(DEPTNO)
);

DROP TABLE IF EXISTS DEPT2;
CREATE TABLE DEPT2 (
	DEPTNO NUMERIC NOT NULL,
	DNAME TEXT NULL,
	ACTIVE NUMERIC NULL,
	PRIMARY KEY(DEPTNO)
);

DROP TABLE IF EXISTS EMP;
CREATE TABLE EMP (
	EMPNO INTEGER NOT NULL,
	ENAME TEXT NULL,
	JOB TEXT,
	MGR INTEGER NULL,
	HIREDATE TIMESTAMP NULL,
	SAL INTEGER NULL,
	COMM NUMERIC NULL,
	DEPTNO NUMERIC NULL,
	TSTAMP TIMESTAMP NULL,
	PRIMARY KEY(EMPNO)
);

DROP TABLE IF EXISTS EMP2;
CREATE TABLE EMP2 (
	EMPNO INTEGER NOT NULL,
	ENAME TEXT NULL,
	DEPTNUM NUMERIC NULL,
	PRIMARY KEY(EMPNO)
);

DROP TABLE IF EXISTS EMP_NULLABLE;
CREATE TABLE EMP_NULLABLE (
	EMPNO INTEGER NOT NULL,
	ENAME TEXT NULL,
	JOB TEXT,
	MGR INTEGER NULL,
	HIREDATE TIMESTAMP NULL,
	SAL INTEGER NULL,
	COMM TEXT NULL,
	DEPTNO NUMERIC NULL,
	TSTAMP TIMESTAMP NULL,
	NULLABLENEXTRESTDATE TIMESTAMP NULL,
	PRIMARY KEY(EMPNO)
);

DROP TABLE IF EXISTS GENERIC_NULLABLE;
CREATE TABLE GENERIC_NULLABLE (
	ID INTEGER NOT NULL,
	DDATE TIMESTAMP NULL,
	ENTITYNO INTEGER NULL,
	PRIMARY KEY(ID)
);

DROP TABLE IF EXISTS UNDER_SCORE;
CREATE TABLE UNDER_SCORE (
	UNDER_SCORE_NO NUMERIC NOT NULL,
	TABLE_NAME TEXT,
	TABLE_NAME_ TEXT,
	_TABLE_NAME TEXT,
	_TABLE_NAME_ TEXT,
	PRIMARY KEY(UNDER_SCORE_NO)
);

DROP TABLE IF EXISTS IDTABLE;
CREATE TABLE IDTABLE (
	ID INTEGER NOT NULL,
	ID_NAME TEXT,
	PRIMARY KEY(ID)
);

DROP TABLE IF EXISTS BASICTYPE;
CREATE TABLE BASICTYPE (
	ID NUMERIC NOT NULL,
	BOOLTYPE INTEGER NULL,
	BYTETYPE NUMERIC NULL,
	SBYTETYPE NUMERIC NULL,
	INT16TYPE NUMERIC NULL,
	INT32TYPE INTEGER NULL,
	INT64TYPE NUMERIC NULL,
	SINGLETYPE NUMERIC NULL,
	DOUBLETYPE NUMERIC NULL,
	DECIMALTYPE NUMERIC NULL,
	STRINGTYPE TEXT NULL,
	DATETIMETYPE TIMESTAMP NULL,
	PRIMARY KEY(ID)
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
INSERT INTO DEPT VALUES (10, 'ACCOUNTING', 'NEW YORK', 0, 1);
INSERT INTO DEPT VALUES (20, 'RESEARCH',   'DALLAS',   0, 1);
INSERT INTO DEPT VALUES (30, 'SALES',      'CHICAGO',  0, 1);
INSERT INTO DEPT VALUES (40, 'OPERATIONS', 'BOSTON',   0, 1);
INSERT INTO EMP2 VALUES (7369, 'SMITH', 20);
INSERT INTO EMP2 VALUES (7499, 'ALLEN', 30);
INSERT INTO EMP_NULLABLE VALUES (1, 'ADAMS', 'CLERK', 7788, '1983-01-12 00:00:00', 1100, NULL, 20, '2000-01-01 00:00:00', '2006-11-07 00:00:00');
INSERT INTO EMP_NULLABLE VALUES (10, 'JAMES', 'CLERK', 7698, '1981-12-03 00:00:00', 950, NULL, 30, '2000-01-01 00:00:00', null);
INSERT INTO EMP_NULLABLE VALUES (100, 'FORD', 'ANALYST', 7566, '1981-12-03 00:00:00', 3000, NULL, 20, '2000-01-01 00:00:00', '2006-11-07 00:00:00');
INSERT INTO GENERIC_NULLABLE (DDATE, ENTITYNO) VALUES ('2000-12-31 12:34:56', 100);
INSERT INTO GENERIC_NULLABLE (DDATE, ENTITYNO) VALUES (null, 101);
INSERT INTO UNDER_SCORE VALUES (1,'table_name','table_name_','_table_name','_table_name_');
INSERT INTO DEPT2 VALUES (20, 'RESEARCH', 1);
INSERT INTO DEPT2 VALUES (30, 'SALES',    0);
INSERT INTO BASICTYPE VALUES (
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
    '�|\\���`',
    '1980-12-17 12:34:56'
);
INSERT INTO BASICTYPE (
    id
) VALUES (
    2
);
INSERT INTO BASICTYPE VALUES (
    3,
    1,
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