const checks = document.getElementById('checks');
const output = document.getElementById('output');
const buttons = [...document.querySelectorAll('button')];
let running = false;

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
	const response = await fetch(path, { credentials: 'include', cache: 'no-store', ...options });
	return { response, body: await readBody(response) };
}

async function postLogout() {
	return request('/sample/logout', { method: 'POST' });
}

async function getPrincipal() {
	return request('/identity/principal');
}

async function getAdminArea() {
	return request('/account/admin-area');
}

async function postLogin(name, secret) {
	return request('/identity/login', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ method: 'local.secret', name, secret })
	});
}

async function expectAnonymousPrincipal(label) {
	const { response, body } = await getPrincipal();
	writeResponse(body);
	return addCheck(label, response.ok && body?.isAuthenticated === false);
}

async function expectAuthenticatedPrincipal(label, role, permission) {
	const { response, body } = await getPrincipal();
	writeResponse(body);
	return addCheck(label,
		response.ok
		&& body?.isAuthenticated === true
		&& body?.roles?.includes(role)
		&& (permission === null || body?.permissions?.includes(permission)));
}

async function expectAdminRejected(label) {
	const { response, body } = await getAdminArea();
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck(label, response.status === 401 || response.status === 403);
}

async function expectAdminAllowed() {
	const { response, body } = await getAdminArea();
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck('admin principal can access account.manage endpoint', response.ok && body?.requiredPermission === 'account.manage');
}

async function expectLogin(name, label) {
	const { response, body } = await postLogin(name, 'password');
	writeResponse(body);
	return addCheck(label, response.ok && body?.isAuthenticated === true);
}

async function expectWrongPasswordRejected() {
	const { response, body } = await postLogin('admin', 'wrong');
	writeResponse(body ?? `HTTP ${response.status}`);
	return addCheck('wrong password is rejected', response.status === 401);
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
	}
}

async function showManualResult(label, action) {
	// Manual buttons intentionally do not write PASS/FAIL test results because they depend on current cookie state.
	const { response, body } = await action();
	writeResponse({ action: label, status: response.status, body });
}

document.getElementById('all').addEventListener('click', () => runExclusive(async () => {
	// The deterministic scenario owns every session transition so auth checks cannot interleave with stale cookies.
	await postLogout();
	await expectAnonymousPrincipal('initial principal is anonymous');
	await expectAdminRejected('anonymous admin access is rejected');

	await expectLogin('operator', 'operator login succeeds');
	await expectAuthenticatedPrincipal('operator principal is authenticated without account.manage', 'operator', null);
	await expectAdminRejected('operator cannot access account.manage endpoint');

	await postLogout();
	await expectLogin('admin', 'admin login succeeds');
	await expectAuthenticatedPrincipal('admin principal has admin role and account.manage permission', 'admin', 'account.manage');
	await expectAdminAllowed();

	await expectWrongPasswordRejected();
	await postLogout();
	await expectAnonymousPrincipal('logout returns to anonymous principal');
}, true));

document.getElementById('manual-login-admin').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('login as admin', () => postLogin('admin', 'password'));
}));

document.getElementById('manual-login-operator').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('login as operator', () => postLogin('operator', 'password'));
}));

document.getElementById('manual-logout').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('logout', postLogout);
}));

document.getElementById('manual-principal').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('show current principal', getPrincipal);
}));

document.getElementById('manual-admin').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('try admin area', getAdminArea);
}));

document.getElementById('manual-wrong').addEventListener('click', () => runExclusive(async () => {
	await showManualResult('try wrong password', () => postLogin('admin', 'wrong'));
}));
