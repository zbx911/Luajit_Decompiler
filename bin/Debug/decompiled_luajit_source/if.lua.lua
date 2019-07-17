if (2 < 1) then
	---Block---
	(KSHORT): 0, 1, 0;
	(KSHORT): 1, 2, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (1 < 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 0, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (2 <= 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 1, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 2, 0;
	(ISGT): 1, 0, 0;
	---End Block---
	

end

if (2 <= 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 2, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 2, 0;
	(ISGT): 1, 0, 0;
	---End Block---
	

end

if (2 == 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 3, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(ISNEN): 0, 0, 0;
	---End Block---
	

end

if (2 ~= 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 4, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(ISEQN): 0, 0, 0;
	---End Block---
	

end

if not (2 < 1) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 5, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 1, 0;
	(KSHORT): 1, 2, 0;
	(ISLT): 1, 0, 0;
	---End Block---
	

end

if not (1 <= 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 6, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISLE): 1, 0, 0;
	---End Block---
	

end

if not (1 < 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 7, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISLT): 1, 0, 0;
	---End Block---
	

end

if not (2 <= 1) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 8, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 1, 0;
	(KSHORT): 1, 2, 0;
	(ISLE): 1, 0, 0;
	---End Block---
	

end

if (2 < 1) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 9, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 1, 0;
	(KSHORT): 1, 2, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (1 < 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 11, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (1 < 2) then
	---Block---
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (1 < 2) then
	---Block---
	(GGET): 0, 0, 0;
	(KSHORT): 1, 14, 0;
	(CALL): 0, 2, 1;
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if (3 < 4) then
	---Block---
	(KSHORT): 0, 4, 0;
	(KSHORT): 1, 3, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

if not (1 < 2) then
	---Block---
	(KSHORT): 0, 2, 0;
	(KSHORT): 1, 1, 0;
	(ISLT): 1, 0, 0;
	---End Block---
	

end

if (3 < 4) then
	---Block---
	(KSHORT): 0, 4, 0;
	(KSHORT): 1, 3, 0;
	(ISGE): 1, 0, 0;
	---End Block---
	

end

