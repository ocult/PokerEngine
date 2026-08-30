const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');
const resultCards = document.querySelector('#result-cards');
const resultRanking = document.querySelector('#result-ranking');
const resultDescription = document.querySelector('#result-description');
const resultRaw = document.querySelector('#result-raw');

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
  const trimmed = cardToken.trim();
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

function renderCards(cardsText) {
  const container = resultCards;
  container.innerHTML = '';

  if (!cardsText) {
    container.textContent = '-';
    return;
  }

  const cards = cardsText.split(',');
  cards.forEach((card) => {
    const token = createCardToken(card);
    if (token && token.nodeType !== 3) {
      container.appendChild(token);
    }
  });
}

function renderError(message) {
  renderCards('');
  resultRanking.textContent = '-';
  resultDescription.textContent = message;
  resultRaw.textContent = message;
  handResult.classList.add('is-error');
}

function renderSuccess(payload) {
  renderCards(payload.cards ?? '');
  resultRanking.textContent = payload.ranking ?? '-';
  resultDescription.textContent = payload.description ?? '-';
  resultRaw.textContent = JSON.stringify(payload, null, 2);
  handResult.classList.remove('is-error');
}

if (handForm) {
  handForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const cards = document.querySelector('#cards').value;
    const response = await fetch('/api/hand', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({ cards })
    });

    const payload = await response.json();

    if (!response.ok) {
      renderError(payload.error ?? 'Falha ao avaliar a mão.');
      return;
    }

    renderSuccess(payload);
  });
}
