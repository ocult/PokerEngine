const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');

function renderError(message) {
  handResult.innerHTML = '';
  handResult.classList.add('is-error');
  handResult.appendChild(createHandSummaryBlock('Mão', '-', message, ''));
  handResult.appendChild(createResultRawBlock('Resposta da API', message));
}

function renderSuccess(payload) {
  handResult.innerHTML = '';
  handResult.classList.remove('is-error');
  handResult.appendChild(createHandSummaryBlock(
    'Mão',
    payload.ranking ?? '-',
    payload.description ?? '-',
    payload.cards ?? ''
  ));
  handResult.appendChild(createResultRawBlock('Resposta da API', JSON.stringify(payload, null, 2)));
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
