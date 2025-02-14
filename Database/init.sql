-- User-Tabelle
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    elo INT DEFAULT 100,
    coins INT DEFAULT 20,
    wins INT DEFAULT 0,
    losses INT DEFAULT 0
);

-- Cards-Tabelle
CREATE TABLE cards (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    damage INT NOT NULL,
    type TEXT NOT NULL,
    owner_id INT REFERENCES users(id)
);

-- Decks-Tabelle
CREATE TABLE decks (
    user_id INT REFERENCES users(id),
    card_id UUID REFERENCES cards(id),
    PRIMARY KEY (user_id, card_id)
);

-- Trades-Tabelle
CREATE TABLE trades (
    id TEXT PRIMARY KEY,
    owner_id INT REFERENCES users(id),
    offered_card_id UUID REFERENCES cards(id),
    required_type TEXT NOT NULL,
    minimum_damage INT NOT NULL
);

INSERT INTO users (username, password) VALUES ('Ahmet', 'secure123'), ('Mehmet', 'secret456');