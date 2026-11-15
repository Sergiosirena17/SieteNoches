import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const pptxgen = require('pptxgenjs');
const PDFDocument = require('pdfkit');
const ShapeType = new pptxgen().ShapeType;

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const output = path.join(root, 'Docs', 'Presentacion');
const captures = path.join(root, 'Docs', 'Capturas');
const pptxPath = path.join(output, '7_Nights_on_Mars_NASA_Space_Apps_2026.pptx');
const pdfPath = path.join(output, '7_Nights_on_Mars_NASA_Space_Apps_2026.pdf');

const W = 13.333;
const H = 7.5;
const PW = 960;
const PH = 540;
const C = {
  night: '121A2E', purple: '2C006B', yellow: 'F2C14E', red: 'E5484D',
  cream: 'F4F1EA', green: '4CAF7A', blue: '2F4B7C', panel: '1C2742',
  muted: 'B9C3D3', black: '08101F', mars: 'B4532A', white: 'FFFFFF',
};

const img = (name) => path.join(captures, name);
const shots = {
  title: img('Inicio.png'), intro: img('Intro.png'), map: img('Juego_02_Mapa.png'),
  telemetry: img('Juego_01_Noche1_1830.png'), greenhouse: img('Juego_03_Invernadero.png'),
  life: img('Juego_04_SoporteVital.png'), medical: img('Juego_05_Biomedica.png'),
  antenna: img('Juego_07_Antena.png'), dawn: img('Juego_06_Amanecer.png'),
  night7: img('Juego_09_Noche7.png'), launch: img('Juego_10_Lanzamiento.png'), final: img('Final.png'),
};

for (const file of Object.values(shots)) {
  if (!fs.existsSync(file)) throw new Error(`Missing capture: ${file}`);
}

function addPptText(slide, text, x, y, w, h, options = {}) {
  slide.addText(text, {
    x, y, w, h, margin: 0, breakLine: false, fontFace: 'Aptos',
    color: C.cream, fontSize: 16, valign: 'mid', fit: 'shrink',
    ...options,
  });
}

function pptRect(slide, x, y, w, h, color, transparency = 0, radius = false) {
  slide.addShape(radius ? ShapeType.roundRect : ShapeType.rect, {
    x, y, w, h, rectRadius: radius ? 0.08 : undefined,
    fill: { color, transparency }, line: { color, transparency: 100 },
  });
}

function pptLine(slide, x1, y1, x2, y2, color, width = 1, transparency = 0) {
  slide.addShape(ShapeType.line, { x: x1, y: y1, w: x2 - x1, h: y2 - y1,
    line: { color, width, transparency } });
}

function pptImage(slide, file, x, y, w, h) {
  slide.addImage({ path: file, x, y, w, h });
}

function pptHeader(slide, number, title, kicker) {
  pptRect(slide, 0, 0, W, H, C.night);
  pptRect(slide, 0, 0, W, 0.12, C.yellow);
  addPptText(slide, String(number).padStart(2, '0'), 0.48, 0.3, 0.55, 0.32,
    { fontSize: 11, bold: true, color: C.yellow, charSpace: 1.5 });
  addPptText(slide, kicker.toUpperCase(), 1.12, 0.3, 3.2, 0.32,
    { fontSize: 9, bold: true, color: C.muted, charSpace: 1.1 });
  addPptText(slide, title, 0.48, 0.7, 7.4, 0.62,
    { fontSize: 28, bold: true, color: C.cream });
  pptLine(slide, 0.48, 1.42, 12.85, 1.42, C.blue, 1);
}

function pptBrand(slide, x, y, w, h) {
  pptRect(slide, x, y, w, h, C.purple, 0, true);
  slide.addShape(ShapeType.ellipse, { x: x + 0.25, y: y + 0.32, w: h * 0.56, h: h * 0.56,
    fill: { color: C.purple, transparency: 100 }, line: { color: C.white, width: 1.3 } });
  slide.addShape(ShapeType.ellipse, { x: x + 0.54, y: y + 0.23, w: h * 0.18, h: h * 0.18,
    fill: { color: C.purple, transparency: 100 }, line: { color: C.white, width: 1.1 } });
  for (const [dx, dy] of [[0.78, 0.25], [0.85, 0.35], [0.89, 0.46]]) {
    pptLine(slide, x + dx, y + dy, x + dx + 0.16, y + dy - 0.13, C.white, 1);
  }
  addPptText(slide, 'NASA', x + 1.25, y + 0.24, w - 1.45, 0.22,
    { fontSize: 8, bold: true, color: C.red, charSpace: 4, align: 'center' });
  addPptText(slide, 'SPACE APPS', x + 1.13, y + 0.47, w - 1.24, 0.35,
    { fontSize: 17, bold: true, color: C.white, charSpace: 1.3, align: 'center' });
  addPptText(slide, 'CHALLENGE', x + 1.17, y + 0.85, w - 1.29, 0.2,
    { fontSize: 7, bold: true, color: C.white, charSpace: 3.2, align: 'center' });
}

function addPptDeck() {
  const ppt = new pptxgen();
  ppt.layout = 'LAYOUT_WIDE';
  ppt.author = '7 Nights on Mars team';
  ppt.company = 'UCSP';
  ppt.subject = 'NASA Space Apps Challenge — Space Mission Design Game';
  ppt.title = '7 Nights on Mars';
  ppt.lang = 'en-US';
  ppt.theme = { headFontFace: 'Aptos Display', bodyFontFace: 'Aptos', lang: 'en-US' };
  ppt.defineLayout({ name: 'WIDE_CUSTOM', width: W, height: H });
  ppt.layout = 'WIDE_CUSTOM';
  ppt.defineSlideMaster({
    title: 'MASTER', background: { color: C.night },
    objects: [{ rect: { x: 0, y: 7.37, w: W, h: 0.13, fill: { color: C.purple }, line: { color: C.purple } } }],
    slideNumber: { x: 12.55, y: 7.08, color: C.muted, fontSize: 8 },
  });

  // 1. Title
  {
    const s = ppt.addSlide('MASTER');
    pptImage(s, shots.title, 0, 0, W, H);
    pptRect(s, 0, 0, W, H, C.black, 32);
    pptRect(s, 0, 0, 0.14, H, C.yellow);
    addPptText(s, 'NASA SPACE APPS CHALLENGE 2026', 0.7, 0.62, 5.9, 0.28,
      { fontSize: 10, bold: true, color: C.yellow, charSpace: 1.8 });
    addPptText(s, '7 Nights\non Mars', 0.68, 1.14, 6.3, 1.62,
      { fontSize: 40, bold: true, color: C.cream, breakLine: true, breakLine: false });
    addPptText(s, 'A playable mission trainer about the systems that keep a Mars habitat alive.', 0.72, 2.98, 5.55, 0.7,
      { fontSize: 17, color: C.cream, breakLine: false });
    pptRect(s, 0.7, 4.25, 4.82, 0.76, C.panel, 8, true);
    addPptText(s, 'Space Mission Design Game', 0.98, 4.44, 4.25, 0.26,
      { fontSize: 15, bold: true, color: C.yellow, align: 'center' });
    pptBrand(s, 9.05, 5.43, 3.55, 1.3);
  }

  // 2. Challenge
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 2, 'The Challenge', 'NASA Space Apps Challenge 2026');
    pptBrand(s, 9.46, 0.34, 3.1, 1.0);
    addPptText(s, 'Designing a space mission means balancing competing demands.', 0.58, 1.82, 6.1, 0.58,
      { fontSize: 23, bold: true, color: C.cream });
    const cards = [
      ['MISSION', 'Objectives and timing'], ['SYSTEMS', 'Power, mass and life support'],
      ['CHOICES', 'Every decision shapes success'],
    ];
    cards.forEach((card, i) => {
      const x = 0.6 + i * 2.12;
      pptRect(s, x, 3.02, 1.86, 1.18, i === 2 ? C.purple : C.panel, 0, true);
      addPptText(s, card[0], x + 0.16, 3.26, 1.54, 0.2, { fontSize: 9, bold: true, color: C.yellow, charSpace: 1.2, align: 'center' });
      addPptText(s, card[1], x + 0.15, 3.57, 1.55, 0.35, { fontSize: 12, color: C.cream, align: 'center' });
    });
    pptImage(s, shots.intro, 7.35, 1.76, 5.38, 3.02);
    pptRect(s, 7.35, 4.43, 5.38, 0.36, C.purple, 0);
    addPptText(s, 'From abstract trade-offs to an interactive mission.', 7.55, 4.49, 4.98, 0.17,
      { fontSize: 10, bold: true, color: C.cream, align: 'center' });
    addPptText(s, 'Our response: a game where students design, manage and simulate a seven-night Mars mission.', 0.6, 5.08, 11.4, 0.55,
      { fontSize: 18, color: C.muted, align: 'center', italic: true });
  }

  // 3. Problem
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 3, 'The Problem: Every System Is Connected', 'Why Mars survival is hard');
    pptImage(s, shots.map, 6.62, 1.72, 6.12, 3.44);
    pptRect(s, 6.62, 1.72, 6.12, 3.44, C.black, 45);
    addPptText(s, 'A safe habitat is not one decision.\nIt is a chain of connected decisions.', 0.6, 1.94, 5.28, 1.12,
      { fontSize: 25, bold: true, color: C.cream, breakLine: false });
    const systemRows = [
      ['ENERGY', 'Limited battery at night', C.yellow], ['LIFE SUPPORT', 'O₂, CO₂, heat and water', C.green],
      ['CREW', 'Stress, hunger and health', C.red], ['MISSION', 'Rescue or a self-sustaining dome', C.purple],
    ];
    systemRows.forEach((row, i) => {
      const y = 3.5 + i * 0.55;
      pptRect(s, 0.62, y, 1.55, 0.33, row[2], 0, true);
      addPptText(s, row[0], 0.69, y + 0.08, 1.4, 0.12, { fontSize: 8, bold: true, color: C.white, align: 'center' });
      addPptText(s, row[1], 2.35, y + 0.04, 3.8, 0.2, { fontSize: 13, color: C.cream });
    });
    addPptText(s, 'One system can protect—or endanger—another.', 6.9, 5.45, 5.58, 0.42,
      { fontSize: 16, bold: true, color: C.yellow, align: 'center' });
  }

  // 4. Solution
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 4, 'Our Solution: Learn by Playing', 'A 2D browser mission trainer');
    pptImage(s, shots.telemetry, 0.58, 1.7, 7.1, 3.99);
    pptRect(s, 0.58, 5.3, 7.1, 0.39, C.purple, 0);
    addPptText(s, 'Every night starts with new data, a new incident and a new decision.', 0.82, 5.4, 6.62, 0.14,
      { fontSize: 10, bold: true, color: C.cream, align: 'center' });
    const steps = [
      ['01', 'Read', 'Telemetry and incident'], ['02', 'Decide', 'Visit mission stations'],
      ['03', 'Reflect', 'See the overnight consequences'],
    ];
    steps.forEach((step, i) => {
      const y = 1.88 + i * 1.28;
      pptRect(s, 8.25, y, 0.64, 0.64, C.yellow, 0, true);
      addPptText(s, step[0], 8.29, y + 0.21, 0.56, 0.12, { fontSize: 11, bold: true, color: C.night, align: 'center' });
      addPptText(s, step[1], 9.1, y + 0.04, 2.5, 0.23, { fontSize: 18, bold: true, color: C.cream });
      addPptText(s, step[2], 9.1, y + 0.34, 3.05, 0.2, { fontSize: 12, color: C.muted });
    });
    addPptText(s, 'A complete mission takes 15–25 minutes.', 8.25, 5.25, 3.8, 0.24,
      { fontSize: 14, bold: true, color: C.green, align: 'center' });
  }

  // 5. Scope
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 5, 'Mission Scope: Four Connected Stations', 'Each station changes the others');
    const cards = [
      [shots.greenhouse, 'GREENHOUSE', 'Grow food and oxygen'], [shots.life, 'LIFE SUPPORT', 'Run MOXIE, heat and fans'],
      [shots.medical, 'MEDICAL TERMINAL', 'Support crew health'], [shots.antenna, 'X-BAND ANTENNA', 'Send rescue messages'],
    ];
    cards.forEach((card, i) => {
      const x = 0.55 + (i % 2) * 6.25;
      const y = 1.7 + Math.floor(i / 2) * 2.55;
      pptImage(s, card[0], x, y, 5.9, 1.82);
      pptRect(s, x, y + 1.43, 5.9, 0.39, C.black, 20);
      addPptText(s, card[1], x + 0.2, y + 1.48, 2.55, 0.13, { fontSize: 10, bold: true, color: C.yellow, charSpace: 1 });
      addPptText(s, card[2], x + 2.52, y + 1.48, 3.1, 0.13, { fontSize: 10, color: C.cream, align: 'right' });
    });
    addPptText(s, 'Time advances after every visit; your settings persist overnight.', 0.7, 6.92, 11.95, 0.22,
      { fontSize: 14, bold: true, color: C.green, align: 'center' });
  }

  // 6. Learning
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 6, 'Learning Through Trade-offs', 'Consequences make the science tangible');
    pptImage(s, shots.dawn, 7.1, 1.72, 5.62, 3.16);
    addPptText(s, 'At 06:00, the mission resolves what the player left configured.', 0.62, 1.86, 5.8, 0.66,
      { fontSize: 24, bold: true, color: C.cream });
    const outcomes = [
      ['BATTERY', 'Power can fail before dawn'], ['OXYGEN', 'MOXIE and plants matter'],
      ['CO₂ & HEALTH', 'Crew choices have consequences'], ['FOOD & WATER', 'Growing systems take planning'],
    ];
    outcomes.forEach((outcome, i) => {
      const y = 3.05 + i * 0.58;
      pptRect(s, 0.64, y, 1.75, 0.32, i % 2 ? C.green : C.yellow, 0, true);
      addPptText(s, outcome[0], 0.72, y + 0.09, 1.58, 0.1, { fontSize: 8, bold: true, color: C.night, align: 'center' });
      addPptText(s, outcome[1], 2.65, y + 0.05, 3.8, 0.18, { fontSize: 12, color: C.cream });
    });
    pptRect(s, 7.1, 4.58, 5.62, 0.3, C.purple, 0);
    addPptText(s, 'NASA-inspired data informs the rules behind the choices.', 7.32, 4.65, 5.16, 0.12,
      { fontSize: 9, bold: true, color: C.cream, align: 'center' });
    addPptText(s, 'The game separates real science facts from the fictional story through in-game data cards.', 0.64, 5.82, 11.9, 0.42,
      { fontSize: 15, color: C.muted, align: 'center', italic: true });
  }

  // 7. Final challenge
  {
    const s = ppt.addSlide('MASTER');
    pptHeader(s, 7, 'The Final Challenge', 'Night 7: 60 real seconds');
    pptImage(s, shots.night7, 0.58, 1.73, 6.1, 3.43);
    pptImage(s, shots.launch, 6.9, 1.73, 5.82, 3.43);
    pptRect(s, 0.58, 1.73, 6.1, 3.43, C.red, 72);
    pptRect(s, 6.9, 1.73, 5.82, 3.43, C.black, 52);
    addPptText(s, 'CLOSE THE VALVES', 1.07, 3.09, 5.1, 0.26,
      { fontSize: 22, bold: true, color: C.white, align: 'center', charSpace: 1 });
    addPptText(s, 'REACH THE BRIDGE', 7.36, 3.09, 4.9, 0.26,
      { fontSize: 22, bold: true, color: C.yellow, align: 'center', charSpace: 1 });
    pptRect(s, 0.95, 5.55, 11.45, 0.72, C.panel, 0, true);
    addPptText(s, 'Seven nights of accumulated decisions lead to one of eight endings—and a Scientific Mission Report.', 1.2, 5.78, 10.95, 0.2,
      { fontSize: 15, bold: true, color: C.cream, align: 'center' });
  }

  // 8. Impact
  {
    const s = ppt.addSlide('MASTER');
    pptImage(s, shots.final, 0, 0, W, H);
    pptRect(s, 0, 0, W, H, C.night, 20);
    pptRect(s, 0.52, 0.52, 7.05, 5.92, C.black, 22, true);
    addPptText(s, 'Impact and Next Steps', 0.92, 0.93, 5.8, 0.5, { fontSize: 29, bold: true, color: C.cream });
    addPptText(s, 'Make Mars survival systems understandable, memorable and discussable.', 0.92, 1.7, 5.85, 0.65,
      { fontSize: 19, color: C.yellow, bold: true });
    const next = ['Final art and audio', 'External playtesting', 'Web build and public release'];
    next.forEach((text, i) => {
      pptRect(s, 0.95, 2.75 + i * 0.65, 0.28, 0.28, C.green, 0, true);
      addPptText(s, '✓', 0.99, 2.8 + i * 0.65, 0.2, 0.12, { fontSize: 10, bold: true, color: C.night, align: 'center' });
      addPptText(s, text, 1.45, 2.77 + i * 0.65, 4.65, 0.18, { fontSize: 15, color: C.cream });
    });
    addPptText(s, 'Thank you.', 0.92, 5.32, 2.5, 0.38, { fontSize: 24, bold: true, color: C.cream });
    pptBrand(s, 9.14, 5.45, 3.47, 1.25);
  }

  return ppt.writeFile({ fileName: pptxPath });
}

function hex(color) { return `#${color}`; }
function pdfRect(doc, x, y, w, h, color, opacity = 1, radius = 0) {
  doc.save().fillOpacity(opacity).fillColor(hex(color));
  if (radius) doc.roundedRect(x, y, w, h, radius).fill(); else doc.rect(x, y, w, h).fill();
  doc.restore();
}
function pdfText(doc, text, x, y, w, size, color = C.cream, opts = {}) {
  doc.fillColor(hex(color)).font(opts.bold ? 'Helvetica-Bold' : 'Helvetica').fontSize(size);
  if (opts.italic) doc.font('Helvetica-Oblique');
  doc.text(text, x, y, { width: w, align: opts.align ?? 'left', lineGap: opts.lineGap ?? 2 });
}
function pdfImage(doc, file, x, y, w, h) { doc.image(file, x, y, { width: w, height: h }); }
function pdfHeader(doc, number, title, kicker) {
  pdfRect(doc, 0, 0, PW, PH, C.night);
  pdfRect(doc, 0, 0, PW, 9, C.yellow);
  pdfText(doc, String(number).padStart(2, '0'), 35, 24, 45, 9, C.yellow, { bold: true });
  pdfText(doc, kicker.toUpperCase(), 82, 24, 240, 8, C.muted, { bold: true });
  pdfText(doc, title, 35, 54, 580, 25, C.cream, { bold: true });
  doc.save().strokeColor(hex(C.blue)).lineWidth(1).moveTo(35, 103).lineTo(925, 103).stroke().restore();
}
function pdfBrand(doc, x, y, w, h) {
  pdfRect(doc, x, y, w, h, C.purple, 1, 9);
  doc.save().lineWidth(2).strokeColor(hex(C.white)).ellipse(x + 20, y + 25, h * 0.28, h * 0.28).stroke();
  doc.ellipse(x + 48, y + 19, h * 0.09, h * 0.09).stroke();
  [[73,27,87,16],[80,36,94,25],[84,46,99,35]].forEach(([a,b,c,d]) => doc.moveTo(x+a,y+b).lineTo(x+c,y+d).stroke());
  doc.restore();
  pdfText(doc, 'NASA', x + 90, y + 18, w - 105, 8, C.red, { bold: true, align: 'center' });
  pdfText(doc, 'SPACE APPS', x + 80, y + 36, w - 92, 17, C.white, { bold: true, align: 'center' });
  pdfText(doc, 'CHALLENGE', x + 82, y + 61, w - 94, 7, C.white, { bold: true, align: 'center' });
}
function addPdfPage(doc) { doc.addPage({ size: [PW, PH], margin: 0 }); }

function addPdfDeck() {
  const doc = new PDFDocument({ autoFirstPage: false, margin: 0, info: { Title: '7 Nights on Mars', Author: '7 Nights on Mars team' } });
  const out = fs.createWriteStream(pdfPath);
  doc.pipe(out);

  addPdfPage(doc); pdfImage(doc, shots.title, 0, 0, PW, PH); pdfRect(doc, 0, 0, PW, PH, C.black, 0.32); pdfRect(doc, 0, 0, 10, PH, C.yellow);
  pdfText(doc, 'NASA SPACE APPS CHALLENGE 2026', 50, 45, 420, 9, C.yellow, { bold: true });
  pdfText(doc, '7 Nights\non Mars', 50, 85, 430, 40, C.cream, { bold: true, lineGap: -6 });
  pdfText(doc, 'A playable mission trainer about the systems that keep a Mars habitat alive.', 52, 214, 380, 17, C.cream);
  pdfRect(doc, 50, 306, 345, 56, C.panel, 0.92, 7); pdfText(doc, 'Space Mission Design Game', 72, 327, 302, 15, C.yellow, { bold: true, align: 'center' }); pdfBrand(doc, 651, 391, 255, 94);

  addPdfPage(doc); pdfHeader(doc, 2, 'The Challenge', 'NASA Space Apps Challenge 2026'); pdfBrand(doc, 680, 27, 230, 72);
  pdfText(doc, 'Designing a space mission means balancing competing demands.', 43, 132, 440, 23, C.cream, { bold: true });
  [['MISSION','Objectives and timing'],['SYSTEMS','Power, mass and life support'],['CHOICES','Every decision shapes success']].forEach((card,i)=>{ const x=43+i*152; pdfRect(doc,x,218,134,85,i===2?C.purple:C.panel,1,8); pdfText(doc,card[0],x+10,235,114,8,C.yellow,{bold:true,align:'center'}); pdfText(doc,card[1],x+10,259,114,11,C.cream,{align:'center'}); });
  pdfImage(doc, shots.intro, 529, 127, 387, 218); pdfRect(doc,529,319,387,26,C.purple); pdfText(doc,'From abstract trade-offs to an interactive mission.',545,327,355,9,C.cream,{bold:true,align:'center'}); pdfText(doc,'Our response: a game where students design, manage and simulate a seven-night Mars mission.',43,366,850,17,C.muted,{align:'center',italic:true});

  addPdfPage(doc); pdfHeader(doc,3,'The Problem: Every System Is Connected','Why Mars survival is hard'); pdfImage(doc,shots.map,496,124,440,248); pdfRect(doc,496,124,440,248,C.black,0.45); pdfText(doc,'A safe habitat is not one decision.\nIt is a chain of connected decisions.',43,140,402,25,C.cream,{bold:true});
  [['ENERGY','Limited battery at night',C.yellow],['LIFE SUPPORT','O₂, CO₂, heat and water',C.green],['CREW','Stress, hunger and health',C.red],['MISSION','Rescue or a self-sustaining dome',C.purple]].forEach((r,i)=>{const y=252+i*40;pdfRect(doc,43,y,112,24,r[2],1,6);pdfText(doc,r[0],49,y+7,100,7,C.white,{bold:true,align:'center'});pdfText(doc,r[1],175,y+5,270,12,C.cream);}); pdfText(doc,'One system can protect—or endanger—another.',518,393,400,15,C.yellow,{bold:true,align:'center'});

  addPdfPage(doc); pdfHeader(doc,4,'Our Solution: Learn by Playing','A 2D browser mission trainer'); pdfImage(doc,shots.telemetry,43,122,511,287); pdfRect(doc,43,381,511,28,C.purple); pdfText(doc,'Every night starts with new data, a new incident and a new decision.',58,390,481,9,C.cream,{bold:true,align:'center'}); [['01','Read','Telemetry and incident'],['02','Decide','Visit mission stations'],['03','Reflect','See the overnight consequences']].forEach((r,i)=>{const y=135+i*92;pdfRect(doc,594,y,46,46,C.yellow,1,6);pdfText(doc,r[0],599,y+16,36,10,C.night,{bold:true,align:'center'});pdfText(doc,r[1],657,y+3,180,18,C.cream,{bold:true});pdfText(doc,r[2],657,y+30,220,12,C.muted);}); pdfText(doc,'A complete mission takes 15–25 minutes.',594,378,275,13,C.green,{bold:true,align:'center'});

  addPdfPage(doc); pdfHeader(doc,5,'Mission Scope: Four Connected Stations','Each station changes the others'); [[shots.greenhouse,'GREENHOUSE','Grow food and oxygen'],[shots.life,'LIFE SUPPORT','Run MOXIE, heat and fans'],[shots.medical,'MEDICAL TERMINAL','Support crew health'],[shots.antenna,'X-BAND ANTENNA','Send rescue messages']].forEach((c,i)=>{const x=40+(i%2)*450,y=122+Math.floor(i/2)*184;pdfImage(doc,c[0],x,y,425,131);pdfRect(doc,x,y+103,425,28,C.black,0.78);pdfText(doc,c[1],x+14,y+111,184,8,C.yellow,{bold:true});pdfText(doc,c[2],x+190,y+111,218,8,C.cream,{align:'right'});});pdfText(doc,'Time advances after every visit; your settings persist overnight.',52,465,856,13,C.green,{bold:true,align:'center'});

  addPdfPage(doc); pdfHeader(doc,6,'Learning Through Trade-offs','Consequences make the science tangible'); pdfImage(doc,shots.dawn,532,124,405,228); pdfText(doc,'At 06:00, the mission resolves what the player left configured.',43,134,430,24,C.cream,{bold:true}); [['BATTERY','Power can fail before dawn'],['OXYGEN','MOXIE and plants matter'],['CO₂ & HEALTH','Crew choices have consequences'],['FOOD & WATER','Growing systems take planning']].forEach((r,i)=>{const y=220+i*42;pdfRect(doc,43,y,126,23,i%2?C.green:C.yellow,1,6);pdfText(doc,r[0],49,y+7,114,7,C.night,{bold:true,align:'center'});pdfText(doc,r[1],188,y+5,275,12,C.cream);});pdfRect(doc,532,330,405,22,C.purple);pdfText(doc,'NASA-inspired data informs the rules behind the choices.',547,337,375,8,C.cream,{bold:true,align:'center'});pdfText(doc,'The game separates real science facts from the fictional story through in-game data cards.',48,419,860,15,C.muted,{align:'center',italic:true});

  addPdfPage(doc); pdfHeader(doc,7,'The Final Challenge','Night 7: 60 real seconds'); pdfImage(doc,shots.night7,43,124,440,248);pdfImage(doc,shots.launch,498,124,419,248);pdfRect(doc,43,124,440,248,C.red,0.72);pdfRect(doc,498,124,419,248,C.black,0.52);pdfText(doc,'CLOSE THE VALVES',79,230,368,21,C.white,{bold:true,align:'center'});pdfText(doc,'REACH THE BRIDGE',530,230,355,21,C.yellow,{bold:true,align:'center'});pdfRect(doc,68,400,824,56,C.panel,1,8);pdfText(doc,'Seven nights of accumulated decisions lead to one of eight endings—and a Scientific Mission Report.',89,421,782,14,C.cream,{bold:true,align:'center'});

  addPdfPage(doc); pdfImage(doc,shots.final,0,0,PW,PH);pdfRect(doc,0,0,PW,PH,C.night,0.20);pdfRect(doc,38,38,507,426,C.black,0.78,8);pdfText(doc,'Impact and Next Steps',66,70,418,28,C.cream,{bold:true});pdfText(doc,'Make Mars survival systems understandable, memorable and discussable.',66,132,420,18,C.yellow,{bold:true});['Final art and audio','External playtesting','Web build and public release'].forEach((t,i)=>{const y=220+i*47;pdfRect(doc,68,y,21,21,C.green,1,5);pdfText(doc,'✓',72,y+5,13,8,C.night,{bold:true,align:'center'});pdfText(doc,t,103,y+2,340,15,C.cream);});pdfText(doc,'Thank you.',66,384,180,23,C.cream,{bold:true});pdfBrand(doc,658,392,249,90);

  doc.end();
  return new Promise((resolve, reject) => { out.on('finish', resolve); out.on('error', reject); });
}

await addPptDeck();
await addPdfDeck();
console.log(`Created ${pptxPath}`);
console.log(`Created ${pdfPath}`);
