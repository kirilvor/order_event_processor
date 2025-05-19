CREATE TABLE IF NOT EXISTS orders
(
    id       TEXT PRIMARY KEY,
    product  TEXT           NOT NULL,
    total    NUMERIC(10, 2) NOT NULL,
    currency TEXT           NOT NULL
);

CREATE TABLE IF NOT EXISTS payments
(
    id       SERIAL PRIMARY KEY,
    order_id TEXT         NOT NULL,
    amount   NUMERIC(10, 2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders (id)
);