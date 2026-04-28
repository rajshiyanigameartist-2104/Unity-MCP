const SIZE = 8;
const TYPES = 6;
const MOVES_START = 25;
const CELL = 64;

const COLORS = [
  ['#ff6b6b', '#ff8787'],
  ['#4dabf7', '#74c0fc'],
  ['#69db7c', '#8ce99a'],
  ['#ffd43b', '#ffe066'],
  ['#b197fc', '#d0bfff'],
  ['#ffa94d', '#ffc078']
];

const canvas = document.getElementById('board');
const ctx = canvas.getContext('2d');
const scoreEl = document.getElementById('score');
const movesEl = document.getElementById('moves');
const restartBtn = document.getElementById('restart');

let grid = [];
let selected = null;
let score = 0;
let moves = MOVES_START;
let busy = false;

restartBtn.addEventListener('click', init);
canvas.addEventListener('pointerdown', onPointerDown);

function init() {
  score = 0;
  moves = MOVES_START;
  selected = null;
  busy = false;
  buildGrid();
  removeInitialMatches();
  draw();
  refreshHud();
}

function refreshHud() {
  scoreEl.textContent = `Score: ${score}`;
  movesEl.textContent = `Moves: ${moves}`;
}

function buildGrid() {
  grid = Array.from({ length: SIZE }, () => Array(SIZE).fill(0));
  for (let y = 0; y < SIZE; y++) {
    for (let x = 0; x < SIZE; x++) {
      grid[y][x] = randType();
    }
  }
}

function randType() {
  return Math.floor(Math.random() * TYPES);
}

function onPointerDown(e) {
  if (busy || moves <= 0) return;
  const rect = canvas.getBoundingClientRect();
  const x = Math.floor((e.clientX - rect.left) / (rect.width / SIZE));
  const y = Math.floor((e.clientY - rect.top) / (rect.height / SIZE));
  if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) return;

  if (!selected) {
    selected = { x, y };
    draw();
    return;
  }

  if (Math.abs(selected.x - x) + Math.abs(selected.y - y) !== 1) {
    selected = { x, y };
    draw();
    return;
  }

  swap(selected, { x, y });
  const matches = findMatches();
  if (!matches.length) {
    swap(selected, { x, y });
    selected = null;
    draw();
    return;
  }

  selected = null;
  moves--;
  busy = true;
  resolveCascade(matches).then(() => {
    busy = false;
    refreshHud();
    draw();
    if (moves <= 0) {
      setTimeout(() => alert(`Game over! Score: ${score}`), 50);
    }
  });
}

function swap(a, b) {
  const t = grid[a.y][a.x];
  grid[a.y][a.x] = grid[b.y][b.x];
  grid[b.y][b.x] = t;
}

function removeInitialMatches() {
  let m = findMatches();
  while (m.length) {
    clearMatches(m);
    collapse();
    refill();
    m = findMatches();
  }
}

function findMatches() {
  const set = new Set();

  for (let y = 0; y < SIZE; y++) {
    let run = 1;
    for (let x = 1; x <= SIZE; x++) {
      if (x < SIZE && grid[y][x] === grid[y][x - 1]) run++;
      else {
        if (run >= 3) {
          for (let k = 0; k < run; k++) set.add(`${x - 1 - k},${y}`);
        }
        run = 1;
      }
    }
  }

  for (let x = 0; x < SIZE; x++) {
    let run = 1;
    for (let y = 1; y <= SIZE; y++) {
      if (y < SIZE && grid[y][x] === grid[y - 1][x]) run++;
      else {
        if (run >= 3) {
          for (let k = 0; k < run; k++) set.add(`${x},${y - 1 - k}`);
        }
        run = 1;
      }
    }
  }

  return [...set].map((s) => {
    const [x, y] = s.split(',').map(Number);
    return { x, y };
  });
}

function clearMatches(matches) {
  for (const m of matches) {
    grid[m.y][m.x] = -1;
  }
  score += matches.length * 50;
}

function collapse() {
  for (let x = 0; x < SIZE; x++) {
    const vals = [];
    for (let y = 0; y < SIZE; y++) {
      if (grid[y][x] !== -1) vals.push(grid[y][x]);
    }
    for (let y = 0; y < SIZE; y++) {
      grid[y][x] = y < vals.length ? vals[y] : -1;
    }
  }
}

function refill() {
  for (let y = 0; y < SIZE; y++) {
    for (let x = 0; x < SIZE; x++) {
      if (grid[y][x] === -1) grid[y][x] = randType();
    }
  }
}

async function resolveCascade(initialMatches) {
  let matches = initialMatches;
  let combo = 0;
  while (matches.length) {
    combo++;
    clearMatches(matches);
    score += combo * 25;
    draw(matches);
    await sleep(120);
    collapse();
    draw();
    await sleep(120);
    refill();
    draw();
    await sleep(120);
    matches = findMatches();
  }
}

function draw(highlight = []) {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  for (let y = 0; y < SIZE; y++) {
    for (let x = 0; x < SIZE; x++) {
      drawCell(x, y, grid[y][x]);
    }
  }

  for (const h of highlight) {
    ctx.strokeStyle = '#fff';
    ctx.lineWidth = 3;
    ctx.strokeRect(h.x * CELL + 4, h.y * CELL + 4, CELL - 8, CELL - 8);
  }

  if (selected) {
    ctx.strokeStyle = '#94d2ff';
    ctx.lineWidth = 4;
    ctx.strokeRect(selected.x * CELL + 3, selected.y * CELL + 3, CELL - 6, CELL - 6);
  }
}

function drawCell(x, y, type) {
  const px = x * CELL;
  const py = y * CELL;

  ctx.fillStyle = '#1a2647';
  roundRect(px + 2, py + 2, CELL - 4, CELL - 4, 14);
  ctx.fill();

  if (type < 0) return;
  const [c1, c2] = COLORS[type];
  const g = ctx.createLinearGradient(px, py, px + CELL, py + CELL);
  g.addColorStop(0, c1);
  g.addColorStop(1, c2);
  ctx.fillStyle = g;
  roundRect(px + 8, py + 8, CELL - 16, CELL - 16, 16);
  ctx.fill();

  ctx.fillStyle = '#ffffffaa';
  ctx.beginPath();
  ctx.ellipse(px + CELL * 0.38, py + CELL * 0.32, 10, 6, -0.6, 0, Math.PI * 2);
  ctx.fill();
}

function roundRect(x, y, w, h, r) {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

init();
