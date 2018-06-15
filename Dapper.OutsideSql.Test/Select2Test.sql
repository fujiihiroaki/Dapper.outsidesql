select 
     EMP.EMPNO      EmpNo
	,EMP.ENAME      Ename
	,EMP22.ENAME    MgrName
	,EMP.JOB        Job
	,DEPT.DNAME Dname	
from 
	(EMP left join DEPT on EMP.DEPTNO = DEPT.DEPTNO) left join EMP as EMP22 on EMP.MGR = EMP22.EMPNO
/*BEGIN*/
where 
	/*IF sarary != null*/    EMP.SAL > /*sarary*/1000
		-- ELSE EMP.SAL > 0
    /*END*/
	/*IF jobnm != null*/ AND EMP.JOB = /*jobnm*/'SALESMAN' /*END*/
	/*IF mgrnm != null*/ AND EMP22.ENAME in /*mgrnm*/('FORD','BLAKE') /*END*/
/*END*/