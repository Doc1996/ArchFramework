const checks = document.getElementById('checks');
const output = document.getElementById('output');
const buttons = [...document.querySelectorAll('button')];
const googleStatus = document.getElementById('google-status');
const googleCallback = document.getElementById('google-callback');
const googleLogin = document.getElementById('google-login');
let running = false;
let jwtToken = null;

function writeResponse(value) {
	output.textContent = typeof value === 'string' ? value : JSON.stringify(value, null, 2);
}

function addCheck(name, passed) {
	const item = document.createElement('li');
	item.textContent = `${passed ? 'PASS' : 'FAIL'}: ${name}`;
	item.className = passed ? 'good' : 'bad';
	checks.append(item);
	return passed;
}

async function readBody(response) {
	const text = await response.text();
	try {
		return text.length === 0 ? null : JSON.parse(text);
	} catch {
		return text;
	}
}

async function request(path, options = {}) {
	const response = await fetch(path, { cache: 'no-store', ...options });
	return { response, body: await readBody(response) };
}

async function issueJwt(secret = 'password') {
	return request('/identity/jwt/login', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ method: 'local.secret', name: 'admin', secret })
	});
}

async function callJwtApi(token) {
	const headers = token === null ? {} : { Authorization: `Bearer ${token}` };
	return request('/api/jwt/admin-area', { headers });
}

async function runExclusive(action, clearChecks = false) {
	if (running)
		return;

	running = true;
	buttons.forEach(button => button.disabled = true);
	if (clearChecks)
		checks.replaceChildren();

	try {
		await action();
	} catch (error) {
		writeResponse(String(error));
		if (clearChecks)
			addCheck('sample request failed', false);
	} finally {
		running = false;
		buttons.forEach(button => button.disabled = false);
		googleLogin.disabled = googleLogin.dataset.configured !== 'true';
	}
}

async function loadGoogleStatus() {
	const { response, body } = await request('/sample/google/status');
	writeResponse(body);

	if (!response.ok) {
		googleStatus.textContent = 'Google status could not be read.';
		googleStatus.className = 'bad';
		googleLogin.disabled = true;
		return;
	}

	googleStatus.textContent = body.message;
	googleStatus.className = body.isConfigured ? 'good' : 'muted';
	googleCallback.textContent = `Google callback URL: ${body.callbackUrl}`;
	googleLogin.dataset.configured = body.isConfigured ? 'true' : 'false';
	googleLogin.disabled = !body.isConfigured;
}

document.getElementById('jwt-checks').addEventListener('click', () => runExclusive(async () => {
	// The deterministic JWT scenario proves token issuing, missing-token rejection, invalid-token rejection, and valid-token authorization.
	const missing = await callJwtApi(null);
	writeResponse(missing.body ?? `HTTP ${missing.response.status}`);
	addCheck('missing bearer token is rejected', missing.response.status === 401);

	const invalid = await callJwtApi('invalid-token');
	writeResponse(invalid.body ?? `HTTP ${invalid.response.status}`);
	addCheck('invalid bearer token is rejected', invalid.response.status === 401);

	const login = await issueJwt();
	writeResponse(login.body);
	jwtToken = login.body?.accessToken ?? null;
	addCheck('local credential login issues JWT', login.response.ok && Boolean(jwtToken));

	const allowed = await callJwtApi(jwtToken);
	writeResponse(allowed.body ?? `HTTP ${allowed.response.status}`);
	addCheck('valid JWT can access protected API', allowed.response.ok && allowed.body?.isAuthenticated === true);

	const wrong = await issueJwt('wrong');
	writeResponse(wrong.body ?? `HTTP ${wrong.response.status}`);
	addCheck('wrong password cannot issue JWT', wrong.response.status === 401 || wrong.response.status === 400);
}, true));

document.getElementById('jwt-login').addEventListener('click', () => runExclusive(async () => {
	const result = await issueJwt();
	jwtToken = result.body?.accessToken ?? null;
	writeResponse({ status: result.response.status, body: result.body, tokenStoredInBrowserVariable: Boolean(jwtToken) });
}));

document.getElementById('jwt-call').addEventListener('click', () => runExclusive(async () => {
	const result = await callJwtApi(jwtToken);
	writeResponse({ status: result.response.status, body: result.body });
}));

document.getElementById('jwt-missing').addEventListener('click', () => runExclusive(async () => {
	const result = await callJwtApi(null);
	writeResponse({ status: result.response.status, body: result.body });
}));

document.getElementById('jwt-invalid').addEventListener('click', () => runExclusive(async () => {
	const result = await callJwtApi('invalid-token');
	writeResponse({ status: result.response.status, body: result.body });
}));

googleLogin.addEventListener('click', () => {
	window.location.href = '/identity/google/login';
});

loadGoogleStatus();
