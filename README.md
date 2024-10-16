<img src="./docs/Logo.png" alt="StartScene" width="100px">

# 로얄캣 (Royal Cat) 
## Ⅰ. 프로젝트 소개
### 개요
- 남녀노소 쉽게 즐길 수 있는 3D 배틀 로얄 슈팅 게임

### 기간 및 인원
- 2024.08 ~ 2024.10 (약 2개월)
- 6인 (클라이언트 4인, 서버 2인)

### 게임 스토리
### 고양이 왕국의 왕이 되기 위한 6마리 고양이의 생선 튀기는 싸움이 시작된다!

```bash
지구로부터 멀리 떨어진 고양이 별의 고양이 왕국.

그 곳에는 모든 고양이들이 평화롭게 살아가는 왕국이 존재하고 있었다.

어느날, 고양이 왕국의 고양이 왕이 갑작스럽게 죽은 뒤, 왕의 유서가
공개되었는데..!

“용맹한 6마리의 고양이 중 가장 강한 고양이가 다음 왕이 될 것이다“

과연 고양이 왕국의 다음 왕이 될 고양이는 누가 될 것인가!!! 빰빠빰
```

<br>

## Ⅱ. 게임 소개


#### [로얄캣 홈페이지 주소](http://j11b309.p.ssafy.io/) 

### 01. 게임 화면

#### 시작 화면

<img src="./docs/Title.gif" alt="Title" width="600px">
- Start 버튼 클릭 시 로그인 화면으로 넘어감

<br/>

#### 로그인 화면
<img src="./docs/Login.png" alt="Login" width="600px">
- 로그인을 통해 게임 로비로 입장

<br/>

#### 로비 화면

<img src="./docs/Lobby.PNG" alt="Lobby" width="600px">
- 로비에서 대기방 생성 및 찾기 가능
- 공개방과 비밀방으로 구분되며 비밀방은 비밀번호를 입력해야 입장 가능

<br/>

#### 대기방 화면

<img src="./docs/Room.PNG" alt="Room" width="600px">
- 방 인원들은 Ready 버튼으로 게임 준비
- 방장은 인원들이 모두 Ready 버튼 클릭 시 Start 버튼으로 게임 시작 가능
- 플레이할 맵과 캐릭터 스킨을 고를 수 있음

<br/>

#### 인게임 화면
<img src="./docs/InGame.png" alt="InGame" width="600px">

<br/>
<br/>

### 02. 플레이어 기본 조작 🏃‍♀️

#### 플레이어 이동

<img src="./docs/move.gif" alt="move" width="300px">
 WASD 또는 방향키를 통해 상하좌우로 이동 가능

<br/>

#### 구르기

<img src="./docs/dodge.gif" alt="dodge" width="300px">
 SPACE BAR 키를 통해 빠른 속도로 구르기 가능

<br/>

#### 공격

<img src="./docs/attack.gif" alt="attack" width="300px">
 마우스로 투사체를 조준 후 클릭하여 발사 가능, 상대방 피격 시 데미지 부여

<br/>
<br/>

### 03. 인게임 요소

#### 아이템 상자

<img src="./docs/itemBox.gif" alt="itemBox" width="300px">
아이템 상자를 피격하여 투사체, 버프 아이템 획득

<br/>

#### 부쉬

<img src="./docs/bush.gif" alt="bush" width="300px">
부쉬 진입 시 타 플레이어에게 보이지 않는 은신 기능 제공

<br/>

#### 몬스터

플레이어를 괴롭히며 처치 시 스킬샷과 같은 아이템 획득

<br/>
<br/>

### 04. 아이템

<img src="./docs/Items.png" alt="Item" width="600px">

#### 버프

- 플레이어가 획득 시 능력치가 증가하는 아이템
- 체력 회복, 공격력 증가, 방어력 증가, 이동속도 증가

#### 투사체

- 플레이어를 공격할 때 특수효과가 발동하는 생선 아이템
- 문어(시야 차단), 랍스터(넉백), 해파리(이동 반전), 거북이(느려짐), 꽃게(출혈)

#### 스킬샷

- 플레이어가 획득 시 투사체 발사 방식을 변경하는 아이템
- 멀티샷, 포물선 샷

<br/>
<br/>

## Ⅲ. 기술 스택

### SERVER

- Photon PUN2
- Photon CHAT
- SpringBoot3
- MySQL

### Front-End

- Unity
- React.js

### Infra

- AWS Redis
- Nginx
- AWS EC2
- Docker

### Tools

- GitLab
- Notion
- Jira
- Figma
- MatterMost

<br>
<br/>

## Ⅳ. 산출물

### 📃 ERD

### ⚙ 아키텍쳐

<img src="./docs/Architecture.png" alt="Architecture" width="600px">

<br/>
<br>

## V. 팀원 소개 및 회고

<img src="./docs/Team.png" alt="Team" width="600px">

### 진현지

### 정원빈

### 김성현

### 현준호

### 오성윤

### 오승준
