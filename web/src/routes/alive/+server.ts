import { json } from '@sveltejs/kit';

/** @type {import('./$types').RequestHandler} */
export function GET() {
  return json(
    {
      status: 'alive',
      time: new Date().toISOString()
    },
    { status: 200 }
  );
}

export function HEAD() {
  return new Response(null, { status: 200 });
}
