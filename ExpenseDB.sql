create database ExpenseDB
use ExpenseDB
create table Transactions
(
    id int identity primary key,
    title varchar(50) ,
    description varchar(100) ,
    amount decimal(10,2),
    date date ,
    
)
select * from Transactions