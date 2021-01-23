x = 1
y = 1
if x == 1 then
	y = 2
end
if y == 2 then
	y = 2
else
	x = 1
end
if x == 1 then
	if x == 2 then
		x = 3
	else
		x = 4
	end
else
	if x == 1 then
		x = 5
	else
		if x == 3 then
			x = 6
		else
			if x == 9 then
				x = 7
			else
				if x == 33 then
					x = 8
				else
					x = 100
				end
			end
		end
	end
end