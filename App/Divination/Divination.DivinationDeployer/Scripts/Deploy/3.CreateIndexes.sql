--user
create unique index uidx_user_login 
	on "user"("login") where not is_deleted;

create index idx_user_name
    on "user"("name");

create index idx_user_formula_id
    on "user"(formula_id);

--user
create unique index uidx_user_settings_userid 
	on "user_settings"("userid");

create index idx_user_settings_userid
    on "user_settings"("userid");

--product
create unique index uidx_product_name_userid_parentid 
	on product("name", userid, parent_id) where not is_deleted;

create index idx_product_name
    on product("name");

create index idx_product_add_period
    on product(add_period);

create index idx_product_is_leaf
    on product(is_leaf);

create index idx_product_last_add_date
    on product(last_add_date);

create index idx_product_userid
    on product(userid);

--formula
create unique index uidx_formula_name
	on formula("name") where not is_deleted;

create index idx_formula_name
    on formula("name");

--incoming
create index idx_incoming_income_date
    on incoming(income_date, userid);

create index idx_incoming_userid
    on incoming(userid);

--outgoing
create index idx_outgoing_income_date
    on outgoing(out_date, userid);

create index idx_outgoing_product_id
    on outgoing(product_id, userid);

create index idx_outgoing_userid
    on outgoing(userid);

--reserve
create unique index uidx_reserve_userid_product_id
	on reserve(product_id, userid) where not is_deleted;

create index idx_reserve_product_id
    on reserve(product_id, userid);
