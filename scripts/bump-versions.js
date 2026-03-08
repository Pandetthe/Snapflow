const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const rootDir = path.resolve(__dirname, '..');
const propsPath = path.join(rootDir, 'server', 'Directory.Build.props');

function getCurrentVersion() {
    if (fs.existsSync(propsPath)) {
        const content = fs.readFileSync(propsPath, 'utf8');
        const match = content.match(/<Version>(.*?)<\/Version>/);
        return match ? match[1] : '0.0.0';
    }
    return '0.0.0';
}

function getNextCalVer(currentVersion) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1; // 1-12

    const parts = currentVersion.split('.');
    if (parts.length < 3) return `${year}.${month}.0`;

    const lastYear = parseInt(parts[0]);
    const lastMonth = parseInt(parts[1]);
    const lastPatch = parseInt(parts[2]);

    if (lastYear === year && lastMonth === month) {
        return `${year}.${month}.${lastPatch + 1}`;
    } else {
        return `${year}.${month}.0`;
    }
}

let version = process.argv[2];

if (!version || !/^\d{4}\.\d{1,2}\.\d+$/.test(version)) {
    const current = getCurrentVersion();
    version = getNextCalVer(current);
}

console.log(`Bumping versions to ${version}...`);

if (fs.existsSync(path.join(rootDir, 'web', 'package.json'))) {
    execSync(`npm version ${version} --no-git-tag-version`, { cwd: path.join(rootDir, 'web'), stdio: 'inherit' });
}

if (fs.existsSync(propsPath)) {
    let props = fs.readFileSync(propsPath, 'utf8');
    props = props.replace(/<Version>.*?<\/Version>/, `<Version>${version}</Version>`);
    fs.writeFileSync(propsPath, props);
}

execSync(`npm version ${version} --no-git-tag-version`, { cwd: rootDir, stdio: 'inherit' });

console.log(`Successfully bumped to ${version}`);
