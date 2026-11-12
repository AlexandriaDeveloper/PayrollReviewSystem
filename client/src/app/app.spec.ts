import { DOCUMENT } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should enforce Arabic language (lang="ar") and RTL direction (dir="rtl") on document and layout container', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();

    const doc = TestBed.inject(DOCUMENT);
    expect(doc.documentElement.getAttribute('lang')).toBe('ar');
    expect(doc.documentElement.getAttribute('dir')).toBe('rtl');

    const compiled = fixture.nativeElement as HTMLElement;
    const mainContainer = compiled.querySelector('main.container');
    expect(mainContainer).toBeTruthy();
    expect(mainContainer?.getAttribute('dir')).toBe('rtl');
    expect(mainContainer?.getAttribute('lang')).toBe('ar');
  });

  it('should render Arabic scaffold title', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('تهيئة نظام مراجعة المرتبات');
  });

  it('should display technical status without employee data', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('SQL Server 2014');
    expect(compiled.textContent).toContain('NOT_DESIGNED / NO_TABLES');
  });
});
