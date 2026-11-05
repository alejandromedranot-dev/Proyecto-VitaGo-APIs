
-- 1 CREAR BASE DE DATOS Y LUEGO UTILIZARLA
create database GestorEmpleados

go
use GestorEmpleados

go

-- 2 CREAR TABLAS DENTRO DE NUESTRA BASE DE DATOS


create table Empleado(
IdEmpleado int primary key identity,
NombreCompleto varchar(50),
Correo varchar(50),
Sueldo decimal(10,2),
FechaContrato date
)

go
insert into Empleado(NombreCompleto,Correo,Sueldo,FechaContrato)
values
('Maria Mendez','maria@gmail.com',4500,'2024-01-12')
select * from Empleado

go


create procedure sp_listaEmpleados
as
begin
	
	select 
	IdEmpleado,
	NombreCompleto,
	Correo,
	Sueldo,
	convert(char(10),FechaContrato,103)[FechaContrato]
	from Empleado 
	
end

go

create procedure sp_obtenerEmpleado
(
@IdEmpleado int
)
as
begin
	
	select 
	IdEmpleado,
	NombreCompleto,
	Correo,
	Sueldo,
	convert(char(10),FechaContrato,103)[FechaContrato]
	from Empleado where IdEmpleado = @IdEmpleado
	
end

go

create procedure sp_crearEmpleado(
@NombreCompleto varchar(50),
@Correo varchar(50),
@Sueldo decimal(10,2),
@FechaContrato varchar(10),
@numeroError int output,
@respuesta varchar(1000) output
)
as
begin
	set nocount on;
	set dateformat dmy;

	begin try
		insert into Empleado
		(
			NombreCompleto,
			Correo,
			Sueldo,
			FechaContrato
		)
		values
		(
			@NombreCompleto,
			@Correo,
			@Sueldo,
			convert(date,@FechaContrato)
		);

		set @numeroError = 0;
		set @respuesta = 'Empleado agregado correctamente';
	end try
	begin catch
		set @numeroError = error_number();
		set @respuesta = error_message();
	end catch
end

go

create procedure sp_editarEmpleado(
@IdEmpleado int,
@NombreCompleto varchar(50),
@Correo varchar(50),
@Sueldo decimal(10,2),
@FechaContrato varchar(10),
@numeroError int output,
@respuesta varchar(1000) output
)
as
begin
	set nocount on;
	set dateformat dmy;

	begin try
		update Empleado
		set
			NombreCompleto = @NombreCompleto,
			Correo = @Correo,
			Sueldo = @Sueldo,
			FechaContrato = convert(date,@FechaContrato)
		where IdEmpleado = @IdEmpleado;

		set @numeroError = 0;
		set @respuesta = 'Empleado actualizado correctamente';
	end try
	begin catch
		set @numeroError = error_number();
		set @respuesta = error_message();
	end catch
end

go

create procedure sp_eliminarEmpleado(
@IdEmpleado int,
@numeroError int output,
@respuesta varchar(1000) output
)
as
begin
	set nocount on;

	begin try
		delete from Empleado where IdEmpleado = @IdEmpleado;

		set @numeroError = 0;
		set @respuesta = 'Empleado eliminado correctamente';
	end try
	begin catch
		set @numeroError = error_number();
		set @respuesta = error_message();
	end catch
end
