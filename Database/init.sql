-- User-Tabelle
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    elo INT DEFAULT 100,
    coins INT DEFAULT 20,
    wins INT DEFAULT 0,
    losses INT DEFAULT 0
);

-- Cards-Tabelle
CREATE TABLE IF NOT EXISTS cards (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    damage INT NOT NULL,
    type TEXT NOT NULL,
    owner_id INT REFERENCES users(id)
);

-- Decks-Tabelle
CREATE TABLE IF NOT EXISTS decks (
    user_id INT REFERENCES users(id),
    card_id UUID REFERENCES cards(id),
    PRIMARY KEY (user_id, card_id)
);

-- Trades-Tabelle
CREATE TABLE IF NOT EXISTS trades (
    id TEXT PRIMARY KEY,
    owner_id INT REFERENCES users(id),
    offered_card_id UUID REFERENCES cards(id),
    required_type TEXT NOT NULL,
    minimum_damage INT NOT NULL
);

-- Packages-Tabelle (NEU)
CREATE TABLE IF NOT EXISTS packages (
    id SERIAL PRIMARY KEY,
    card1_id UUID REFERENCES cards(id),
    card2_id UUID REFERENCES cards(id),
    card3_id UUID REFERENCES cards(id),
    card4_id UUID REFERENCES cards(id),
    card5_id UUID REFERENCES cards(id)
);

-- Benutzer erstellen
INSERT INTO users (username, password) VALUES 
    ('Ahmet', 'secure123'),
    ('Mehmet', 'secret456');

-- Karten hinzufügen
INSERT INTO cards (id, name, damage, type) VALUES
  ('550e8400-e29b-41d4-a716-446655440000', 'Fire Dragon', 100, 'Monster'),
  ('550e8400-e29b-41d4-a716-446655440001', 'Water Goblin', 50, 'Monster'),
  ('550e8400-e29b-41d4-a716-446655440002', 'Earth Golem', 60, 'Monster'),
  ('550e8400-e29b-41d4-a716-446655440003', 'Air Phoenix', 75, 'Monster'),
  ('550e8400-e29b-41d4-a716-446655440004', 'Lightning Spell', 85, 'Spell');

-- Paket mit 5 Karten hinzufügen
INSERT INTO packages (card1_id, card2_id, card3_id, card4_id, card5_id) VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    '550e8400-e29b-41d4-a716-446655440001',
    '550e8400-e29b-41d4-a716-446655440002',
    '550e8400-e29b-41d4-a716-446655440003',
    '550e8400-e29b-41d4-a716-446655440004'
);
