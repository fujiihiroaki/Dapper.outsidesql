select 
     empno      EmpNo
	,ENAME      Ename
	,EMP2.ENAME MgrName
	,JOB        Job
	,DEPT.DNAME Dname	
from 
	(EMP join DEPT on EMP.DEPTNO = DEPT.DEPTNO) join EMP EMP2 on EMP.MGR = EMP2.EMPNO
/*BEGIN*/
where 
	/*IF sarary != null*/    EMP.SAL > /*sarary*/1000
		-- ELSE EMP.SAL > 0
    /*END*/
	/*IF jobnm != null*/ AND JOB = /*jobnm*/'SALESMAN' /*END*/
	/*IF mgrnm != null*/ AND EMP2.ENAME in /*mgrnm*/('FORD','BLAKE') /*END*/
/*END*/