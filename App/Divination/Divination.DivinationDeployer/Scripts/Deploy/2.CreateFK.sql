--product
alter table product 
add constraint fk_product_user_id 
	foreign key(userid) 
		references "user"(id) 
		on delete no action on update no action;

alter table product 
add constraint fk_product_parent_id 
	foreign key(parent_id) 
		references product(id) 
		on delete no action on update no action;


--formula
alter table "user" 
add constraint fk_user_formula_id 
	foreign key(formula_id) 
		references formula(id) 
		on delete no action on update no action;

--incoming
alter table incoming 
add constraint fk_incoming_user_id 
	foreign key(userid) 
		references "user"(id) 
		on delete no action on update no action;


--outgoing
alter table outgoing 
add constraint fk_outgoing_user_id 
	foreign key(userid) 
		references "user"(id) 
		on delete no action on update no action;

alter table outgoing 
add constraint fk_outgoing_product_id 
	foreign key(product_id) 
		references product(id) 
		on delete no action on update no action;


--reserve
alter table reserve 
add constraint fk_reserve_user_id 
	foreign key(userid) 
		references "user"(id) 
		on delete no action on update no action;

alter table reserve 
add constraint fk_reserve_product_id 
	foreign key(product_id) 
		references product(id) 
		on delete no action on update no action;