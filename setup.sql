/*
    GADGET HUB - ELECTRONICS STORE DATABASE INITIALIZATION SCRIPT
    This script creates the necessary tables and populates them with categories and electronic products.
*/

-- 1. Create Core Tables
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'categories')
BEGIN
    CREATE TABLE dbo.categories(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [name] VARCHAR(255) NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'products')
BEGIN
    CREATE TABLE dbo.products(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        category_id INT NOT NULL REFERENCES dbo.categories(id),
        [name] VARCHAR(255) NOT NULL,
        [description] VARCHAR(MAX) NOT NULL,
        stock INT NOT NULL DEFAULT (0),
        price DECIMAL(10,2) NOT NULL,
        [image] VARCHAR(255) NULL,
        is_show BIT NOT NULL DEFAULT (1),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'notifications')
BEGIN
    CREATE TABLE dbo.notifications(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        recipient_member_id INT NULL REFERENCES dbo.members(id),
        is_admin BIT NOT NULL DEFAULT (0),
        order_id INT NULL REFERENCES dbo.orders(id),
        title VARCHAR(200) NOT NULL,
        body VARCHAR(MAX) NULL,
        is_read BIT NOT NULL DEFAULT (0),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        read_at DATETIME2(0) NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'admins')
BEGIN
    CREATE TABLE dbo.admins(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        full_name VARCHAR(255) NULL,
        username VARCHAR(255) NOT NULL UNIQUE,
        [password] VARCHAR(255) NOT NULL,
        role VARCHAR(50) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'payment_methods')
BEGIN
    CREATE TABLE dbo.payment_methods(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [name] VARCHAR(100) NOT NULL UNIQUE,
        is_use BIT NOT NULL DEFAULT (1),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_payment_methods_name' AND parent_object_id = OBJECT_ID('dbo.payment_methods'))
    ALTER TABLE dbo.payment_methods DROP CONSTRAINT CK_payment_methods_name;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_payment_methods_name' AND parent_object_id = OBJECT_ID('dbo.payment_methods'))
    ALTER TABLE dbo.payment_methods WITH CHECK ADD CONSTRAINT CK_payment_methods_name
        CHECK ([name] IN ('Cash On Delivery', 'Card', 'Bank', 'eSewa'));

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'members')
BEGIN
    CREATE TABLE dbo.members(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        full_name VARCHAR(255) NULL,
        username VARCHAR(255) NOT NULL UNIQUE,
        email VARCHAR(255) NOT NULL UNIQUE,
        [password] VARCHAR(255) NOT NULL,
        phone VARCHAR(50) NULL,
        persistent_token VARCHAR(64) NULL,
        token_expires DATETIME2(0) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'orders')
BEGIN
    CREATE TABLE dbo.orders(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        order_code VARCHAR(50) NOT NULL UNIQUE,
        member_id INT NOT NULL REFERENCES dbo.members(id),
        ship_name VARCHAR(255) NULL,
        ship_phone VARCHAR(50) NULL,
        [status] VARCHAR(50) NULL,
        total_qty INT NOT NULL,
        total_amount DECIMAL(10,2) NOT NULL,
        payment VARCHAR(255) NOT NULL,
        order_date DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'order_items')
BEGIN
    CREATE TABLE dbo.order_items(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        order_id INT NOT NULL REFERENCES dbo.orders(id),
        product_id INT NOT NULL REFERENCES dbo.products(id),
        quantity INT NOT NULL,
        amount DECIMAL(10,2) NOT NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'order_addresses')
BEGIN
    CREATE TABLE dbo.order_addresses(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        order_id INT NOT NULL REFERENCES dbo.orders(id),
        [address] VARCHAR(MAX) NULL,
        township VARCHAR(255) NULL,
        postal_code VARCHAR(20) NULL,
        city VARCHAR(100) NULL,
        [state] VARCHAR(100) NULL,
        country VARCHAR(100) NULL,
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'order_logs')
BEGIN
    CREATE TABLE dbo.order_logs(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        order_id INT NOT NULL REFERENCES dbo.orders(id),
        [status] VARCHAR(50) NOT NULL,
        admin_id INT NOT NULL REFERENCES dbo.admins(id),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'feedbacks')
BEGIN
    CREATE TABLE dbo.feedbacks(
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        member_id INT NULL REFERENCES dbo.members(id),
        admin_id INT NULL REFERENCES dbo.admins(id),
        [name] VARCHAR(255) NULL,
        email VARCHAR(255) NULL,
        title VARCHAR(255) NOT NULL,
        [message] VARCHAR(MAX) NOT NULL,
        reply VARCHAR(MAX) NULL,
        is_resolved BIT NOT NULL DEFAULT (0),
        created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
        updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
    );
END;

-- 2. Seed Data
-- Clear existing data if needed (Optional: uncomment below)
-- DELETE FROM dbo.order_items; DELETE FROM dbo.orders; DELETE FROM dbo.products; DELETE FROM dbo.categories;

-- Categories
INSERT INTO dbo.categories ([name]) VALUES ('Smartphones');
INSERT INTO dbo.categories ([name]) VALUES ('Laptops');
INSERT INTO dbo.categories ([name]) VALUES ('Audio & Headphones');
INSERT INTO dbo.categories ([name]) VALUES ('Cameras');
INSERT INTO dbo.categories ([name]) VALUES ('Accessories');

-- Products
DECLARE @Smartphones INT = (SELECT id FROM categories WHERE [name] = 'Smartphones');
DECLARE @Laptops INT = (SELECT id FROM categories WHERE [name] = 'Laptops');
DECLARE @Audio INT = (SELECT id FROM categories WHERE [name] = 'Audio & Headphones');
DECLARE @Cameras INT = (SELECT id FROM categories WHERE [name] = 'Cameras');
DECLARE @Accessories INT = (SELECT id FROM categories WHERE [name] = 'Accessories');

INSERT INTO dbo.products (category_id, [name], [description], stock, price, is_show) VALUES 
(@Smartphones, 'iPhone 15 Pro', 'Experience the power of titanium and the A17 Pro chip.', 15, 999.00, 1),
(@Smartphones, 'Samsung Galaxy S24 Ultra', 'AI-powered smartphone with 200MP camera and S-Pen.', 18, 1299.00, 1),
(@Laptops, 'MacBook Air M2', 'Thin, light, and incredibly fast with the M2 chip.', 10, 1199.00, 1),
(@Laptops, 'Dell XPS 13', 'Premium design and stunning display for ultimate productivity.', 12, 1099.00, 1),
(@Audio, 'Sony WH-1000XM5', 'Industry-leading noise cancelling headphones.', 25, 349.00, 1),
(@Audio, 'AirPods Pro (2nd Gen)', 'Immersive sound with active noise cancellation.', 30, 249.00, 1),
(@Accessories, 'Logitech MX Master 3S', 'Ergonomic wireless mouse with precision tracking.', 50, 99.00, 1),
(@Accessories, 'USB-C Fast Charger', 'Compact high-speed charger for phones and tablets.', 40, 29.00, 1),
(@Cameras, 'Fujifilm X-T5', 'Beautifully designed mirrorless camera for enthusiasts.', 5, 1699.00, 1),
(@Cameras, 'DJI Mini 4 Pro', 'Pro-level drone in a compact, portable package.', 8, 759.00, 1);

-- Default Admin (Password: 123456 hashed with SHA256)
-- admin / 123456
INSERT INTO dbo.admins (full_name, username, [password], role)
VALUES ('Admin User', 'admin', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 'superadmin');

-- Payment methods used by checkout
DELETE FROM dbo.payment_methods;
INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Cash On Delivery', 1);
INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Card', 1);
INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Bank', 1);
INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('eSewa', 1);

PRINT 'Database setup and seeding completed successfully.';
this is what i am facing in that category The SqlParameter is already contained by another SqlParameterCollection.