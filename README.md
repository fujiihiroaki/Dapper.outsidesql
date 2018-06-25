# Dapper.OutsideSql
Dapper non-public extension for outside sql file.

------------

## Overview

**Dapper.OutsideSql** reads sql statement from text file, formats and passes it to Dapper according to the parameters.  
See format examples below.


## Description
Dapper.OutsideSql does not extend Dapper. The sql statement passing to Dapper is created from **Text file** .  
Dapper.OutsideSql performs the correspondence charge account of the bind variable of the SQL sentence using comment such as /**/ or --. After having done the correspondence charge account, we can use that sql sentence such as SQL Server Management Studio because of sql comment.You should bury comment for it first if you carry out an sql sentence with the tool of the SQL file and come to output a result just as wanted.

## Usage
You can add Dapper, NLog, Microsoft.CodeAnalysis.CSharp.Scripting to your project by [NuGet library](https://www.nuget.org/packages/Dapper).  
Next, add Dapper.OutsideSql to you project reference.

You can create text files, add to you project.  
You should write only one sql sentence to one text file. 


## SQL statement comments  
Sql statement comments follows that of S2Dao.NET. 

### Bind variable  
You can describe bind variable comments in the sql sentence to use the value of the parameters to construct across Dapper in the sql sentence. The bind variable comments and the leterals are replaced with the value of the parameters automatically and are carried out.  
The bind variable comments are writed as follows.

```sql
/*Parameter name*/literal
```

Example:  
```sql 
SELECT * FROM emp WHERE empno = /*empno*/7788
```

### IN phrase  
The bind variable comments are writed as follows.

```sql
IN /*Parameter name*/(..)
```
In the case of IN phrase, the right side literal (dummy data) of parameter name becomes required. Please describe as follows.
```sql
IN /*names*/('aaa','bbb')
```
C#
```csharp
var names = new string[]{"SCOTT", "SMITH", "JAMES"};
```

### IF comment
By the IF comment, You can change an sql sentence to carry out depending on a condition. the IF comment is described as follows.  
```sql
/*IF condition */.../*END*/
```

Example:
```sql
/*IF hoge != null*/hoge = /*hoge*/'abc'/*END*/
```
As for the IF comment, in the case of the true, a part among /*IF*/ and /*END*/ is estimated as condition. In the case of the above, partial (hoge = /*hoge*/'abc') surrounded by the IF comment is used only when parameter hoge is not null. In addition, ELSE comment is prepared for as processing in case of the false. When a condition becomes false, the part which describing after "ELSE" is used. The ELSE comment is described as follows.
```sql
/*IF hoge != null*/hoge = /*hoge*/'abc'
  -- ELSE hoge is null
/*END*/
```

### BEGIN comment
You can use BEGIN comment when you do not want to output WHERE phrase in itself, when all IF comments not to include ELSE comment in the WHERE phrase become false,  BEGIN comment is used in conjunction with IF comment as follows.
```sql
/*BEGIN*/WHERE phrase /*END*/
```

Example:
```sql
/*BEGIN*/WHERE
  /*IF job != null*/job = /*job*/'CLERK'/*END*/
  /*IF deptno != null*/AND deptno = /*deptno*/20/*END*/
/*END*/
```
In the case of the above, the WHERE phrase is not output when job, deptno is null. 


## Examples 1
Text File:
```sql
select mb.MEMBER_ID
     , mb.MEMBER_NAME
     , mb.BIRTHDATE
     , stat.MEMBER_STATUS_NAME
  from MEMBER mb
    left outer join MEMBER_STATUS stat
      on mb.MEMBER_STATUS_CODE = stat.MEMBER_STATUS_CODE
 /*BEGIN*/
 where
   /*IF memberId != null*/
   mb.MEMBER_ID = /*memberId*/3
   /*END*/
   /*IF memberName != null*/
   and mb.MEMBER_NAME like /*memberName*/'S%' -- // keyword for prefix search
   /*END*/
   /*IF birthdate != null*/
   and mb.BIRTHDATE = /*birthdate*/'1966-09-15' -- // used as equal
   /*END*/
 /*END*/
 order by mb.BIRTHDATE desc, mb.MEMBER_ID asc
```
C#:
```csharp
var path = "<text file path>";
var memberList = conn.QueryOutputSql<Hoge>(path, new { memberId = 1, memberName = "hoge%" });
``` 

## Examples 2
C#:
```csharp
var path = "<text file path>";
var param = new DynamicParameters();
param.Add("memberId", 1);
param.Add("memberName", "hoge%");
memberList = conn.QueryOutsideSql<Hoge>(path, param);
``` 

## NLog
Dapper.OutsideSql outputs sql which included parameters are replaced to real values, to NLog, after reading file.
DapperLog also outputs sql to NLog. But, DapperLog don't read sql file.

## Example 3
```csharp
var sql = "select EMP.EMPNO EmpNo,EMP.ENAME Enam from EMP where EMPNO >= /*Empno1*/500 and EMPNO <= /*Empno2*/1000";
var memberList = conn.QueryLog<Hoge>(sql, new { Empno1 = 7900, Empno2 = 7940 });
``` 
```log
 DEBUG Jiifureit.Dapper.OutsideSql.DapperLogExtension._LogSql select EMP.EMPNO EmpNo,EMP.ENAME Enam from EMP where EMPNO >= 7900 and EMPNO <= 7940
``` 

## DB Providers 
- SQL Server
- MySQL
- PostgreSQL
- Oracle  
are tested successfully.

- DB2, odbc, sqlite is not tested.

## License

Dapper.OutsideSql is licensed under the Apache license.  See the [LICENSE file](LICENSE) for more details.


## Thanks, Frameworks

Dapper.OutsideSql forks S2Dao.NET.  
Thanks Dapper, Seasar project and DBFlute project.


## Author

Hiroaki Fujii
