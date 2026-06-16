const fs = require('fs');
const path = require('path');

function getFiles(dir, files_) {
    files_ = files_ || [];
    const files = fs.readdirSync(dir);
    for (let i in files) {
        const name = dir + '/' + files[i];
        if (fs.statSync(name).isDirectory()) {
            if (!name.includes('node_modules') && !name.includes('.git') && !name.includes('obj') && !name.includes('bin')) {
                getFiles(name, files_);
            }
        } else {
            if (name.endsWith('.cs') || name.endsWith('.cshtml')) {
                files_.push(name);
            }
        }
    }
    return files_;
}

const allFiles = getFiles('.');
let totalFixed = 0;

function fixString(s) {
    if (s.includes('Ã') || s.includes('Ä') || s.includes('á') || s.includes('Æ')) {
        try {
            const decoded = Buffer.from(s, 'latin1').toString('utf8');
            if (!decoded.includes('\ufffd')) {
                return decoded;
            }
        } catch (e) {}
    }
    return null;
}

for (const file of allFiles) {
    let originalText = fs.readFileSync(file, 'utf8');
    let fixedText = originalText;

    // Fix strings in quotes
    fixedText = fixedText.replace(/"([^"\\]*(?:\\.[^"\\]*)*)"/g, (match, p1) => {
        const fixed = fixString(p1);
        if (fixed) return '"' + fixed + '"';
        return match;
    });

    if (file.endsWith('.cshtml')) {
        // Fix text between HTML tags
        fixedText = fixedText.replace(/>([^<]+)</g, (match, p1) => {
            const fixed = fixString(p1);
            if (fixed) return '>' + fixed + '<';
            return match;
        });
    }

    if (originalText !== fixedText) {
        fs.writeFileSync(file, fixedText, 'utf8');
        console.log('Fixed ' + file);
        totalFixed++;
    }
}
console.log('Total fixed: ' + totalFixed);
