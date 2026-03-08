import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const rootDir = path.resolve(__dirname, '..');
const webPackagePath = path.join(rootDir, 'web', 'package.json');
const serverPropsPath = path.join(rootDir, 'server', 'Directory.Build.props');

function parseVersion(v) {
    return v.split('.').map(n => parseInt(n, 10) || 0);
}

function compareVersions(v1, v2) {
    const a = parseVersion(v1);
    const b = parseVersion(v2);
    for (let i = 0; i < Math.max(a.length, b.length); i++) {
        const valA = a[i] ?? 0;
        const valB = b[i] ?? 0;
        if (valA > valB) return 1;
        if (valA < valB) return -1;
    }
    return 0;
}

function getCurrentVersions() {
    let versions = [];

    if (fs.existsSync(webPackagePath)) {
        const pkg = JSON.parse(fs.readFileSync(webPackagePath, 'utf8'));
        if (pkg.version) versions.push(pkg.version);
    }

    if (fs.existsSync(serverPropsPath)) {
        const content = fs.readFileSync(serverPropsPath, 'utf8');
        const match = content.match(/<Version>(.*?)<\/Version>/);
        if (match) versions.push(match[1]);
    }

    return versions;
}

function getNextCalVer(currentMax) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;

    let next = `${year}.${month}.0`;

    if (currentMax) {
        const parts = parseVersion(currentMax);
        const lastYear = parts[0];
        const lastMonth = parts[1];
        const lastPatch = parts[2] ?? 0;

        if (lastYear === year && lastMonth === month) {
            next = `${year}.${month}.${lastPatch + 1}`;
        } else if (compareVersions(currentMax, next) >= 0) {
            next = `${parts[0]}.${parts[1]}.${(parts[2] ?? 0) + 1}`;
        }
    }

    return next;
}

let version = process.argv[2];

if (!version || !/^\d{4}\.\d{1,2}\.\d+$/.test(version)) {
    const existing = getCurrentVersions();
    const currentMax = existing.sort(compareVersions).pop();
    version = getNextCalVer(currentMax);
}

console.log(`Smart Synchronizing versions to: ${version}`);

if (fs.existsSync(webPackagePath)) {
    const pkg = JSON.parse(fs.readFileSync(webPackagePath, 'utf8'));
    if (pkg.version !== version) {
        try {
            const webDir = path.join(rootDir, 'web');
            execSync(`npm version ${version} --no-git-tag-version`, { cwd: webDir, stdio: 'inherit' });
        } catch (e) {
            pkg.version = version;
            fs.writeFileSync(webPackagePath, JSON.stringify(pkg, null, 4) + '\n');
        }
    } else {
        console.log(`Web version is already ${version}, skipping npm version.`);
    }
}

if (fs.existsSync(serverPropsPath)) {
    let props = fs.readFileSync(serverPropsPath, 'utf8');
    props = props.replace(/<Version>.*?<\/Version>/, `<Version>${version}</Version>`);
    fs.writeFileSync(serverPropsPath, props);
}

if (process.env.GITHUB_OUTPUT) {
    fs.appendFileSync(process.env.GITHUB_OUTPUT, `new_version=${version}\n`);
}

console.log(`Successfully synchronized all components to ${version}`);
