-- 1.테이블 생성 
CREATE TABLE `players` (
	`player_id` INT NOT NULL AUTO_INCREMENT,
	`username` VARCHAR(50) NOT NULL DEFAULT '0',
	`email` VARCHAR(50) NOT NULL,
	`password_hash` VARCHAR(255) NOT NULL,
	`created_at` TIMESTAMP NOT NULL DEFAULT (CURRENT_TIMESTAMP()),
	`last_login` TIMESTAMP NULL DEFAULT NULL,
	PRIMARY KEY (`player_id`),
	UNIQUE INDEX `username` (`username`),
	UNIQUE INDEX `email` (`email`)
)
COLLATE='utf8mb4_0900_ai_ci'


-- 2. 플레이어 데이터 삽입

INSERT INTO players(username, email, password_hash) VALUES ('hero123','hero123@gmail.com','hashed_password1')
SELECT * FROM `gametest`.`players` LIMIT 1000;

-- 3. 여러개 플레이어 데이터 삽입
INSERT INTO players(username, email, password_hash) VALUES
('hero423','hero323@gmail.com','hashed_password3'),
('hero523','hero423@gmail.com','hashed_password4'),
('hero623','hero523@gmail.com','hashed_password5'),
('hero723','hero623@gmail.com','hashed_password6'),
('hero823','hero723@gmail.com','hashed_password7')

-- 4. 유저 이름 기준으로 최근 로그인 보기
SELECT username, last_login FROM players

-- 5. 특정 플레이어 정보 업데이트
UPDATE players SET last_login = CURRENT_TIMESTAMP WHERE username = 'hero123'

-- 6. 조건에 맞는 플레이어 검색
SELECT username, email FROM players WHERE username LIKE '%hero%'

-- 7. 플레이어 삭제
DELETE FROM players WHERE username = 'hero123'

SELECT * FROM `gametest`.`players` LIMIT 1000;

-- 8. 플레이어 테이블에 새 열 추가 
ALTER TABLE players ADD COLUMN `level` INT DEFAULT 1

-- 9. 모든 플레이어의 레벨을 1증가 
UPDATE players SET `level` = `level` + 
items
-- 10.가장 level 이 높은 플레이어 조회 하기 
SELECT username, `level` FROM players ORDER BY `level` DESC LIMIT 1


-- 아이템 설계 및 제작 
CREATE TABLE items(item_id INT AUTO_INCREMENT PRIMARY KEY, `name` VARCHAR(100) NOT NULL, DESCRIPTION TEXT, VALUE INT DEFAULT 0)

-- 아이템 데이터 삽입
INSERT INTO items(NAME, DESCRIPTION, VALUE) VALUES
('검' , '기본 무기' , 10),
('방패' , '기본 바어구', 15),
('물약', '체력을 회복' , 5)

SELECT * FROM items

-- 플레이어 인벤토리 테이블 생성 
CREATE TABLE inventories(
	inventory_id INT AUTO_INCREMENT PRIMARY KEY,
	player_id INT,
	item_id INT,
	quantity INT DEFAULT 1,
	FOREIGN KEY(player_id) REFERENCES players(player_id),
	FOREIGN KEY(item_id) REFERENCES items(item_id)
)

--인벤토리에 아이템 추가 
INSERT INTO inventories(player_id, item_id, quantity) VALUES
(6,1,1), -- 3번 플레이어에 1번 검 아이템을 1개 추가 
(4,2,5), -- 4번 플레이어에 2번 방패 아이템을 5개 추가 
(5,3,2)  -- 5번 플레이어에 3번 물약 아이템을 2개 추가 

-- 플레이어의 인벤토리 조회 
SELECT p.username, i.name, inv.quantity
FROM players p 
JOIN inventories inv ON p.player_id = inv.player_id
JOIN items i ON inv.item_id = i.item_id

-- 새로운 아이템 추가
INSERT INTO items(NAME , DESCRIPTION, VALUE) VALUES
('용검', '용 기본 무기' , 20)
-- 특정 플레이어의 인벤토리에 새 아이템 추가
INSERT INTO inventories (player_id, item_id, quantity) VALUES (5,4,2)
-- 가장 가치 있는 아이템 찾기 (ORDER BY value DESC)
SELECT NAME,VALUE FROM items ORDER BY VALUE DESC LIMIT 1

-- 퀘스트 테이블 생성 
CREATE TABLE quests(
	quest_id INT AUTO_INCREMENT PRIMARY KEY,
	title VARCHAR(100) NOT NULL,
	DESCRIPTION TEXT,
	reward_exp INT DEFAULT 0,
	reward_item_id INT,
	FOREIGN KEY (reward_item_id) REFERENCES items(item_id)
)

-- 퀘스트 데이터 삽입
INSERT INTO quests(title, DESCRIPTION, reward_exp, reward_item_id) VALUES
('초보자 퀘스트' , '첫번째 퀘스트를 완료 하세요', 100, 3),
('용사의 검', '전설의 검을 찾아보세요' , 500, 1)

-- 플레이어 퀘스트 진행 상황 테이블
CREATE TABLE player_quests(
	player_id INT,
	quest_id INT,
	STATUS ENUM ('시작', '진행중', '완료') DEFAULT '시작',
	start_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP(),
	completed_at TIMESTAMP NULL,
	PRIMARY KEY (player_id, quest_id),
	FOREIGN KEY (player_id) REFERENCES players(player_id),
	FOREIGN KEY (quest_id) REFERENCES quests(quest_id)
)

-- 플레이어에게 퀘스트 할당
INSERT INTO player_quests(player_id, quest_id) VALUES
(4,1),				-- 4번 플레이어에게 1번 퀘스트
(5,2)					-- 5번 플레이어에게 2번 퀘스트 

-- 진행 중인 퀘스트 조회
SELECT p.username, q.title, pq.STATUS
FROM players p
JOIN player_quests pq ON p.player_id = pq.player_id
JOIN quests q ON pq.quest_id = q.quest_id
WHERE pq.STATUS != '완료'
