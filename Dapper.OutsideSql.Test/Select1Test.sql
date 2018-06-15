select 
     EMPNO      EmpNo
	,ENAME      Ename
	,JOB        Job
	,DEPT.DNAME Dname	
from 
	EMP join DEPT on EMP.DEPTNO = DEPT.DEPTNO
/*BEGIN*/
where 
	/*IF sarary != null*/    EMP.SAL > /*sarary*/1000
		-- ELSE EMP.SAL > 0
    /*END*/
	/*IF jobnm != null*/ AND JOB = /*jobnm*/'SALESMAN' /*END*/
/*END*/
order by EMPNO