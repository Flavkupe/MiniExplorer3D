const fs = require('fs');
const path = require('path');
const { S3Client, PutObjectCommand } = require('@aws-sdk/client-s3');
const mime = require('mime');

const BUCKET = 'wiki-museum-webgl-555069934636';
const BUILD_DIR = 'build';

const s3 = new S3Client({ region: 'us-east-2' });

function walk(dir, fileList = []) {
    fs.readdirSync(dir).forEach(file => {
        const filePath = path.join(dir, file);
        if (fs.statSync(filePath).isDirectory()) {
            walk(filePath, fileList);
        } else {
            fileList.push(filePath);
        }
    });
    return fileList;
}

async function uploadFile(localPath, s3Key, extra) {
    const fileContent = fs.readFileSync(localPath);
    const params = {
        Bucket: BUCKET,
        Key: s3Key,
        Body: fileContent,
        ...extra
    };
    await s3.send(new PutObjectCommand(params));
    console.log(`Uploaded: ${s3Key}`);
}

(async () => {
    const allFiles = walk(BUILD_DIR);

    // Upload all except .gz
    for (const file of allFiles) {
        if (file.endsWith('.gz')) continue;
        const s3Key = path.relative(BUILD_DIR, file).replace(/\\/g, '/');
        const contentType = mime.default.getType(file) || 'application/octet-stream';
        await uploadFile(file, s3Key, { ContentType: contentType });
    }

    // Upload .gz files with Content-Encoding: gzip
    for (const file of allFiles) {
        if (!file.endsWith('.gz')) continue;
        const s3Key = path.relative(BUILD_DIR, file).replace(/\\/g, '/').replace(/\.gz$/, '');
        const contentType = mime.default.getType(s3Key) || 'application/octet-stream';
        await uploadFile(file, s3Key, { ContentType: contentType, ContentEncoding: 'gzip' });
    }
})();