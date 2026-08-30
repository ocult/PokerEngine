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
  rankNode.textContent = rank;

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
