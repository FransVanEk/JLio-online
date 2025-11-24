const fs = require('fs');
const path = require('path');

function validateSample(filePath) {
    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const sample = JSON.parse(content);
        
        if (!sample.model || !sample.model.scriptText) {
            return { valid: false, issues: ['Missing model or scriptText'] };
        }

        const script = JSON.parse(sample.model.scriptText);
        const issues = [];

        // Extract path references
        const pathRefs = new Set();
        const targetRefs = new Set();

        script.forEach(cmd => {
            if (cmd.path && cmd.path !== '$') {
                const match = cmd.path.match(/^\$\.([^.\[\]\s\)]+)/);
                if (match) pathRefs.add(match[1]);
            }
            
            if (cmd.targetPath && cmd.targetPath !== '$') {
                const match = cmd.targetPath.match(/^\$\.([^.\[\]\s\)]+)/);
                if (match) targetRefs.add(match[1]);
            }

            ['leftPath', 'rightPath'].forEach(prop => {
                if (cmd[prop] && cmd[prop] !== '$') {
                    const match = cmd[prop].match(/^\$\.([^.\[\]\s\)]+)/);
                    if (match) pathRefs.add(match[1]);
                }
            });

            ['ifTrue', 'ifFalse'].forEach(prop => {
                if (cmd[prop] && Array.isArray(cmd[prop])) {
                    cmd[prop].forEach(nestedCmd => {
                        if (nestedCmd.path && nestedCmd.path !== '$') {
                            const match = nestedCmd.path.match(/^\$\.([^.\[\]\s\)]+)/);
                            if (match) pathRefs.add(match[1]);
                        }
                    });
                }
            });
        });

        // Check input objects
        const inputProps = new Set();
        if (sample.model.inputObjects) {
            sample.model.inputObjects.forEach(inputObj => {
                try {
                    const jsonData = JSON.parse(inputObj.jsonString);
                    if (typeof jsonData === 'object' && jsonData !== null) {
                        Object.keys(jsonData).forEach(key => inputProps.add(key));
                    }
                } catch (e) {
                    issues.push(`Invalid input JSON for ${inputObj.name}: ${e.message}`);
                }
            });
        }

        // Check output objects
        const outputProps = new Set();
        if (sample.model.outputObjects) {
            sample.model.outputObjects.forEach(outputObj => {
                try {
                    const jsonData = JSON.parse(outputObj.jsonString);
                    if (typeof jsonData === 'object' && jsonData !== null) {
                        Object.keys(jsonData).forEach(key => outputProps.add(key));
                    }
                } catch (e) {
                    issues.push(`Invalid output JSON for ${outputObj.name}: ${e.message}`);
                }
            });
        }

        // Check for missing path references
        pathRefs.forEach(ref => {
            if (!inputProps.has(ref)) {
                issues.push(`Missing input property: ${ref}`);
            }
        });

        targetRefs.forEach(ref => {
            if (!outputProps.has(ref)) {
                issues.push(`Missing output property: ${ref}`);
            }
        });

        return {
            valid: issues.length === 0,
            issues,
            pathRefs: Array.from(pathRefs),
            targetRefs: Array.from(targetRefs),
            inputProps: Array.from(inputProps),
            outputProps: Array.from(outputProps),
            sampleName: sample.model.name,
            title: sample.title
        };

    } catch (e) {
        return {
            valid: false,
            issues: [`Parse error: ${e.message}`],
            sampleName: path.basename(filePath),
            title: 'Unknown'
        };
    }
}

function validateAllSamples(samplesDir) {
    const results = [];
    
    function scanDirectory(dir) {
        const items = fs.readdirSync(dir);
        items.forEach(item => {
            const itemPath = path.join(dir, item);
            const stat = fs.statSync(itemPath);
            
            if (stat.isDirectory()) {
                scanDirectory(itemPath);
            } else if (item.match(/^Sample-\d+\.json$/)) {
                results.push({
                    ...validateSample(itemPath),
                    filePath: itemPath,
                    relativePath: path.relative(samplesDir, itemPath)
                });
            }
        });
    }
    
    scanDirectory(samplesDir);
    return results.sort((a, b) => a.sampleName.localeCompare(b.sampleName));
}

// Main execution
const samplesPath = path.join(process.cwd(), 'Client', 'wwwroot', 'samples');

if (!fs.existsSync(samplesPath)) {
    console.log('Samples directory not found:', samplesPath);
    process.exit(1);
}

console.log('Validating samples...\n');

const results = validateAllSamples(samplesPath);
const failedSamples = results.filter(r => !r.valid);
const successCount = results.length - failedSamples.length;

console.log('=== VALIDATION SUMMARY ===');
console.log(`Total samples: ${results.length}`);
console.log(`Successful: ${successCount}`);
console.log(`Failed: ${failedSamples.length}`);

if (failedSamples.length > 0) {
    console.log('\n=== FAILED SAMPLES ===');
    
    const byDirectory = {};
    failedSamples.forEach(sample => {
        const dir = path.dirname(sample.relativePath);
        if (!byDirectory[dir]) byDirectory[dir] = [];
        byDirectory[dir].push(sample);
    });
    
    Object.keys(byDirectory).sort().forEach(dir => {
        console.log(`\n${dir}: ${byDirectory[dir].length} failures`);
        byDirectory[dir].forEach(sample => {
            console.log(`  ? ${sample.sampleName}`);
            sample.issues.forEach(issue => {
                console.log(`     - ${issue}`);
            });
        });
    });
    
    // Export to JSON for further analysis
    fs.writeFileSync('failed_samples.json', JSON.stringify(failedSamples, null, 2));
    console.log('\nFailed samples exported to: failed_samples.json');
}

console.log();