const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');
const resultCards = document.querySelector('#result-cards');
const resultRanking = document.querySelector('#result-ranking');
const resultDescription = document.querySelector('#result-description');
const resultRaw = document.querySelector('#result-raw');

function renderError(message) {
  renderCardRow(resultCards, '');
  resultRanking.textContent = '-';
  resultDescription.textContent = message;
  resultRaw.textContent = message;
  handResult.classList.add('is-error');
}

function renderSuccess(payload) {
  renderCardRow(resultCards, payload.cards ?? '');
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
