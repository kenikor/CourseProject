-- Создание базы данных
CREATE DATABASE Planet35;
GO

USE Planet35;
GO

-- Таблица ролей
CREATE TABLE Planet35_Roles (
    id INT IDENTITY PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);
GO

-- Таблица подразделений
CREATE TABLE Planet35_Departments (
    id INT IDENTITY PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    location NVARCHAR(150) NULL
);
GO

-- Таблица пользователей
CREATE TABLE Planet35_Users (
    id INT IDENTITY PRIMARY KEY,
    username NVARCHAR(100) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(150),
    department_id INT NULL,
    role_id INT NOT NULL,
    FOREIGN KEY (role_id) REFERENCES Planet35_Roles(id),
    FOREIGN KEY (department_id) REFERENCES Planet35_Departments(id)
);
GO

-- Таблица категорий имущества
CREATE TABLE Planet35_AssetCategories (
    id INT IDENTITY PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255) NULL
);
GO

-- Таблица имущества
CREATE TABLE Planet35_Assets (
    id INT IDENTITY PRIMARY KEY,
    inventory_number NVARCHAR(50) NOT NULL UNIQUE,
    name NVARCHAR(150) NOT NULL,
    category_id INT NOT NULL,
    department_id INT NOT NULL,
    responsible_id INT NULL,
    purchase_date DATE NULL,
    cost DECIMAL(18,2) NULL,
    status NVARCHAR(50) DEFAULT 'В эксплуатации',
    FOREIGN KEY (category_id) REFERENCES Planet35_AssetCategories(id),
    FOREIGN KEY (department_id) REFERENCES Planet35_Departments(id),
    FOREIGN KEY (responsible_id) REFERENCES Planet35_Users(id)
);
GO

-- Таблица инвентаризаций
CREATE TABLE Planet35_Inventory (
    id INT IDENTITY PRIMARY KEY,
    conducted_by INT NOT NULL,
    department_id INT NOT NULL,
    date_conducted DATETIME DEFAULT GETDATE(),
    comment NVARCHAR(255) NULL,
    FOREIGN KEY (conducted_by) REFERENCES Planet35_Users(id),
    FOREIGN KEY (department_id) REFERENCES Planet35_Departments(id)
);
GO

-- Таблица позиций инвентаризации
CREATE TABLE Planet35_InventoryItems (
    id INT IDENTITY PRIMARY KEY,
    inventory_id INT NOT NULL,
    asset_id INT NOT NULL,
    status NVARCHAR(50) NOT NULL,
    note NVARCHAR(255) NULL,
    FOREIGN KEY (inventory_id) REFERENCES Planet35_Inventory(id),
    FOREIGN KEY (asset_id) REFERENCES Planet35_Assets(id)
);
GO

-- Таблица журнала изменений
CREATE TABLE Planet35_ChangeLog (
    id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    asset_id INT NULL,
    operation NVARCHAR(50) NOT NULL,
    change_time DATETIME DEFAULT GETDATE(),
    old_value NVARCHAR(MAX) NULL,
    new_value NVARCHAR(MAX) NULL,
    comment NVARCHAR(255) NULL,
    FOREIGN KEY (user_id) REFERENCES Planet35_Users(id),
    FOREIGN KEY (asset_id) REFERENCES Planet35_Assets(id)
);
GO

-- Таблица отчетов
CREATE TABLE Planet35_Reports (
    id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    report_type NVARCHAR(100) NOT NULL,
    file_path NVARCHAR(255) NOT NULL,
    generated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Planet35_Users(id)
);
GO

-- Первичные данные для таблицы ролей
INSERT INTO Planet35_Roles (name) VALUES
('Администратор'),
('Пользователь'),
('Гость');
GO
