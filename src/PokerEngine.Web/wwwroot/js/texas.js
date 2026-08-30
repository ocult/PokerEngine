const texasForm = document.querySelector('#texas-form');
const communityCards = document.querySelector('#community-cards');
const winnerSummary = document.querySelector('#winner-summary');
const playerCards = document.querySelector('#player-cards');
const bestHands = document.querySelector('#best-hands');
const resultRaw = document.querySelector('#result-raw');
const playerCardTemplate = document.querySelector('#player-card-template');
const bestHandTemplate = document.querySelector('#best-hand-template');

function applyPodiumClasses(playerId, medalClass) {
  if (!playerId && playerId !== 0) {
    return;
  }

  const playerBlocks = document.querySelectorAll(`.player-card-box[data-player="${playerId}"]`);
  playerBlocks.forEach((block) => {
    block.classList.remove('podium-gold', 'podium-silver', 'podium-bronze');
    if (medalClass) {
      block.classList.add(medalClass);
    }
  });
}

function createPlayerCard(player, cards, isWinner) {
  const fragment = playerCardTemplate.content.cloneNode(true);
  const card = fragment.querySelector('.player-card-box');
  const title = fragment.querySelector('.player-card-title');
  const cardsContainer = fragment.querySelector('.card-row');

  card.dataset.player = String(player);
  title.textContent = `Jogador ${player}${isWinner ? ' • Campeão' : ''}`;

  cards.forEach((item) => {
    cardsContainer.appendChild(createCardToken(item));
  });

  return fragment;
}

function createBestHandCard(entry, index, winnerPlayer) {
  const fragment = bestHandTemplate.content.cloneNode(true);
  const card = fragment.querySelector('.player-card-box');
  const title = fragment.querySelector('.player-card-title');
  const ranking = fragment.querySelector('.best-hand-ranking');
  const description = fragment.querySelector('.best-hand-description');
  const cardsContainer = fragment.querySelector('.card-row');

  const displayName = entry.player === 0 ? 'Mesa' : `Jogador ${entry.player}`;
  const isWinner = entry.player === winnerPlayer;
  const medalClass = index === 0 ? 'podium-gold' : index === 1 ? 'podium-silver' : index === 2 ? 'podium-bronze' : '';
  const className = medalClass ? `player-card-box ${medalClass}` : 'player-card-box';

  card.dataset.player = String(entry.player);
  card.className = className;
  title.textContent = `${displayName}${isWinner ? ' • Campeão' : ''}`;
  ranking.textContent = entry.ranking || '-';

  const descriptionText = (entry.description || '-').includes('[') && (entry.description || '-').includes(']')
    ? (entry.description || '-').slice(0, (entry.description || '-').lastIndexOf('[')).trim()
    : (entry.description || '-');

  description.textContent = descriptionText;

  const handCards = (entry.description || '').includes('[') && (entry.description || '').includes(']')
    ? (entry.description || '').slice((entry.description || '').indexOf('[') + 1, (entry.description || '').lastIndexOf(']'))
    : '';

  if (entry.player !== 0 && medalClass) {
    applyPodiumClasses(entry.player, medalClass);
  }

  if (handCards) {
    handCards.split(',').forEach((card) => {
      const token = createCardToken(card.trim());
      if (token && token.nodeType !== 3) {
        cardsContainer.appendChild(token);
      }
    });
  }

  return fragment;
}

function renderTexasError(message) {
  renderCardRow(communityCards, '');
  winnerSummary.textContent = message;
  playerCards.innerHTML = '';
  bestHands.innerHTML = '';
  resultRaw.textContent = message;
}

function renderPlayerList(holeCards) {
  if (!holeCards || Object.keys(holeCards).length === 0) {
    playerCards.innerHTML = '<div class="list-item">-</div>';
    return;
  }

  const winnerPlayer = Number(document.querySelector('#winner-summary')?.dataset?.winnerPlayer ?? -1);
  playerCards.innerHTML = '';

  Object.entries(holeCards).forEach(([player, cards]) => {
    const isWinner = Number(player) === winnerPlayer;
    playerCards.appendChild(createPlayerCard(player, cards, isWinner));
  });
}

function renderBestHands(entries) {
  if (!entries || entries.length === 0) {
    bestHands.innerHTML = '<div class="list-item">-</div>';
    return;
  }

  const winnerPlayer = Number(document.querySelector('#winner-summary')?.dataset?.winnerPlayer ?? -1);
  bestHands.innerHTML = '';

  entries.forEach((entry, index) => {
    bestHands.appendChild(createBestHandCard(entry, index, winnerPlayer));
  });
}

function renderTexasSuccess(payload) {
  renderCardRow(communityCards, payload.communityCards?.join(', ') ?? '');

  const winnerPlayer = payload.winner ? payload.winner.player : null;
  winnerSummary.dataset.winnerPlayer = String(winnerPlayer ?? '');
  winnerSummary.textContent = payload.winner ? `${payload.winner.player === 0 ? 'Mesa' : `Jogador ${payload.winner.player}`} — ${payload.winner.ranking}` : '-';

  renderPlayerList(payload.holeCards);
  renderBestHands(payload.bestHands ?? []);
  resultRaw.textContent = JSON.stringify(payload, null, 2);
}

if (texasForm) {
  texasForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const players = document.querySelector('#players').value;
    const response = await fetch('/api/texas-holdem', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({ players })
    });

    const payload = await response.json();

    if (!response.ok) {
      renderTexasError(payload.error ?? 'Falha ao executar o Texas Holdem.');
      return;
    }

    renderTexasSuccess(payload);
  });
}
