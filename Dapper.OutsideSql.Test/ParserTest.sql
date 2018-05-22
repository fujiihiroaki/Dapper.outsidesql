select 
     s_code
	,s_name
from 
	tbl_test
/*BEGIN*/
where 
    /*IF code != null*/s_code = /*code*/'test' /*END*/
	/*IF name != null*/AND s_name = /*name*/'test' /*END*/
/*END*/