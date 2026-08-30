const suitGlyphs = {
  C: '♣',
  H: '♥',
  S: '♠',
  D: '♦'
};

const suitClasses = {
  C: 'suit-clubs',
  H: 'suit-hearts',
  S: 'suit-spades',
  D: 'suit-diamonds'
};

function createCardToken(cardToken) {
  const trimmed = String(cardToken ?? '').trim();
  if (!trimmed) {
    return document.createTextNode('');
  }

  const suit = trimmed[trimmed.length - 1].toUpperCase();
  const rank = trimmed.slice(0, -1).toUpperCase();

  const card = document.createElement('span');
  card.className = `card-token ${suitClasses[suit] ?? ''}`;
  card.title = `${rank}${suit}`;

  const rankNode = document.createElement('span');
  rankNode.className = 'card-rank';
  rankNode.textContent = rank === 'T' ? 10 : rank;

  const suitNode = document.createElement('span');
  suitNode.className = 'card-suit';
  suitNode.textContent = suitGlyphs[suit] ?? suit;

  card.append(rankNode, suitNode);
  return card;
}

function renderCardRow(container, cardsText, fallback = '-') {
  const target = typeof container === 'string' ? document.querySelector(container) : container;
  if (!target) {
    return;
  }

  target.innerHTML = '';

  if (!cardsText || !String(cardsText).trim()) {
    target.textContent = fallback;
    return;
  }

  const cards = String(cardsText).split(',');
  cards.forEach((card) => {
    const token = createCardToken(card);
    if (token && token.nodeType !== 3) {
      target.appendChild(token);
    }
  });
}

function renderTextList(container, items, formatter = (item) => item) {
  const target = typeof container === 'string' ? document.querySelector(container) : container;
  if (!target) {
    return;
  }

  target.innerHTML = '';

  if (!items || items.length === 0) {
    target.textContent = '-';
    return;
  }

  items.forEach((item) => {
    const node = document.createElement('div');
    node.className = 'list-item';
    node.textContent = formatter(item);
    target.appendChild(node);
  });
}

function cloneTemplate(templateSelector) {
  const template = document.querySelector(templateSelector);
  if (!template || !template.content) {
    return document.createDocumentFragment();
  }

  return template.content.cloneNode(true);
}

function createResultCardBlock(labelText, cardsText, isWide = false, templateSelector = '#result-card-template') {
  const fragment = cloneTemplate(templateSelector);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const row = fragment.querySelector('.card-row');

  if (block) {
    block.classList.toggle('result-block-wide', isWide);
  }

  if (label) {
    label.textContent = labelText;
  }

  if (row) {
    renderCardRow(row, cardsText, '-');
  }

  return fragment;
}

function createResultTextBlock(labelText, value, isWide = false, templateSelector = '#result-text-template') {
  const fragment = cloneTemplate(templateSelector);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const content = fragment.querySelector('strong');

  if (block) {
    block.classList.toggle('result-block-wide', isWide);
  }

  if (label) {
    label.textContent = labelText;
  }

  if (content) {
    content.textContent = value ?? '-';
  }

  return fragment;
}

function createResultRawBlock(labelText, value, templateSelector = '#result-raw-template') {
  const fragment = cloneTemplate(templateSelector);
  const block = fragment.querySelector('.result-block');
  const label = fragment.querySelector('.label');
  const pre = fragment.querySelector('pre');

  if (label) {
    label.textContent = labelText;
  }

  if (pre) {
    pre.textContent = value ?? 'Result appears here.';
  }

  if (block) {
    block.classList.add('result-block-wide');
  }

  return fragment;
}

function createHandSummaryBlock(titleText, rankingText, descriptionText, cardsText) {
  const fragment = cloneTemplate('#hand-summary-template');
  const card = fragment.querySelector('.player-card-box');
  const title = fragment.querySelector('.player-card-title');
  const ranking = fragment.querySelector('.best-hand-ranking');
  const description = fragment.querySelector('.best-hand-description');
  const row = fragment.querySelector('.card-row');

  if (card) {
    card.classList.add('player-card-box');
  }

  if (title) {
    title.textContent = titleText || 'My hand';
  }

  if (ranking) {
    ranking.textContent = rankingText || '-';
  }

  if (description) {
    description.textContent = descriptionText || '-';
  }

  if (row) {
    renderCardRow(row, cardsText ?? '', '-');
  }

  return fragment;
}
